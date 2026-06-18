using gschatbot.api.Data;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("agendar")]
public class AgendamentoHandler : IIntentHandler
{
    private readonly AppDbContext _context;
    private readonly TwilioService _twilioService;

    public AgendamentoHandler(AppDbContext context, TwilioService twilioService)
    {
        _context = context;
        _twilioService = twilioService;
    }

    // Round 1: detecta tipo e oferece lista de horários
    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        try
        {
            var dados = llmResponse.Dados;
            var tipo = dados.ContainsKey("tipo") ? dados["tipo"]?.ToString() : null;

            if (tipo == "especialidade")
            {
                var especialidadeNome = dados.ContainsKey("especialidade") ? dados["especialidade"]?.ToString() : null;
                await OfereceHorariosPorEspecialidade(clienteId, numeroWhatsApp, especialidadeNome);
            }
            else if (tipo == "medico")
            {
                var medicoNome = dados.ContainsKey("medico") ? dados["medico"]?.ToString() : null;
                await OfereceHorariosPorMedico(clienteId, numeroWhatsApp, medicoNome);
            }
            else
            {
                // LLM pediu mais informações
                await _twilioService.SendMessage(numeroWhatsApp, llmResponse.Resposta);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgendamentoHandler] Erro: {ex.Message}");
            await _twilioService.SendMessage(numeroWhatsApp, "Erro ao processar agendamento. Tente novamente.");
        }
    }

    // Round 2: cliente escolheu da lista, cria agendamento
    public async Task ProcessarEscolha(int clienteId, string numeroWhatsApp, string mensagem, SessaoConversa sessao)
    {
        try
        {
            ContextoEscolha? ctx = null;
            try { ctx = JsonSerializer.Deserialize<ContextoEscolha>(sessao.ContextoJson); } catch { }

            if (ctx?.Slots == null || ctx.Slots.Count == 0)
            {
                await _twilioService.SendMessage(numeroWhatsApp, "Desculpe, perdi o contexto. Por favor, solicite o agendamento novamente.");
                sessao.EstadoAtual = "inicial";
                sessao.ContextoJson = "{}";
                await _context.SaveChangesAsync();
                return;
            }

            var escolhido = ResolverEscolha(mensagem, ctx.Slots);

            if (escolhido == null)
            {
                var linhas = ctx.Slots.Select(s => $"  {s.Index}. {s.EspecialistaNome} - {s.Data} às {s.Hora}");
                var reenvio = "Desculpe, não entendi. Digite o número da opção desejada:\n\n" +
                              string.Join("\n", linhas);
                await _twilioService.SendMessage(numeroWhatsApp, reenvio);
                return;
            }

            var slot = await _context.HorariosConsulta
                .Include(h => h.Especialista).ThenInclude(e => e.Especialidade)
                .Include(h => h.Endereco)
                .FirstOrDefaultAsync(h => h.Id == escolhido.SlotId && h.Status == "disponivel");

            if (slot == null)
            {
                await _twilioService.SendMessage(numeroWhatsApp,
                    "Desculpe, esse horário acabou de ser ocupado. Solicite novamente para ver horários atualizados.");
                sessao.EstadoAtual = "inicial";
                sessao.ContextoJson = "{}";
                await _context.SaveChangesAsync();
                return;
            }

            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                EspecialistaId = slot.EspecialistaId,
                HorarioConsultaId = slot.Id,
                Status = "confirmado"
            };

            _context.Agendamentos.Add(agendamento);
            slot.Status = "reservado";

            sessao.EstadoAtual = "inicial";
            sessao.ContextoJson = "{}";
            sessao.UltimaMensagemEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var resposta = $"✅ Agendamento confirmado!\n\n" +
                $"📅 Data: {slot.DataConsulta:dd/MM/yyyy}\n" +
                $"⏰ Hora: {slot.HoraInicio:HH:mm}\n" +
                $"👨‍⚕️ Profissional: {slot.Especialista.Nome}\n" +
                $"🏥 Especialidade: {slot.Especialista.Especialidade.Nome}\n" +
                $"📍 Local: {slot.Endereco.Rua}, {slot.Endereco.Numero} - {slot.Endereco.Bairro}, {slot.Endereco.Cidade}\n\n" +
                $"Obrigado!";

            _context.HistoricoMensagens.Add(new HistoricoMensagem
            {
                ClienteId = clienteId,
                AgendamentoId = agendamento.Id,
                RemetenteId = "bot",
                Mensagem = resposta,
                Tipo = "agendamento"
            });

            await _context.SaveChangesAsync();
            await _twilioService.SendMessage(numeroWhatsApp, resposta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgendamentoHandler.ProcessarEscolha] Erro: {ex.Message}");
            await _twilioService.SendMessage(numeroWhatsApp, "Erro ao confirmar agendamento. Tente novamente.");
        }
    }

    private async Task OfereceHorariosPorEspecialidade(int clienteId, string numeroWhatsApp, string? especialidadeNome)
    {
        if (string.IsNullOrEmpty(especialidadeNome))
        {
            await _twilioService.SendMessage(numeroWhatsApp, "Qual especialidade você deseja?");
            return;
        }

        var especialidade = await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Nome.ToLower().Contains(especialidadeNome.ToLower()));

        if (especialidade == null)
        {
            await _twilioService.SendMessage(numeroWhatsApp, $"Não encontrei a especialidade '{especialidadeNome}'. Por favor, verifique o nome.");
            return;
        }

        var especialistas = await _context.Especialistas
            .Where(e => e.EspecialidadeId == especialidade.Id && e.Ativo)
            .ToListAsync();

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var slotsComMedico = new List<(HorarioConsulta Slot, Especialista Especialista)>();

        foreach (var especialista in especialistas)
        {
            var slot = await _context.HorariosConsulta
                .Where(h => h.EspecialistaId == especialista.Id && h.Status == "disponivel" && h.DataConsulta >= hoje)
                .OrderBy(h => h.DataConsulta).ThenBy(h => h.HoraInicio)
                .FirstOrDefaultAsync();

            if (slot != null)
                slotsComMedico.Add((slot, especialista));
        }

        slotsComMedico = slotsComMedico
            .OrderBy(x => x.Slot.DataConsulta).ThenBy(x => x.Slot.HoraInicio)
            .Take(3)
            .ToList();

        if (slotsComMedico.Count == 0)
        {
            await _twilioService.SendMessage(numeroWhatsApp,
                $"Não há horários disponíveis para {especialidade.Nome} no momento.");
            return;
        }

        var slots = slotsComMedico.Select((x, i) => new SlotContexto
        {
            Index = i + 1,
            SlotId = x.Slot.Id,
            EspecialistaNome = x.Especialista.Nome,
            Data = x.Slot.DataConsulta.ToString("dd/MM/yyyy"),
            Hora = x.Slot.HoraInicio.ToString("HH:mm")
        }).ToList();

        await SalvarContextoSessao(clienteId, slots);

        var linhas = slots.Select(s => $"  {s.Index}. {s.EspecialistaNome} - {s.Data} às {s.Hora}");
        var msg = $"Ótimo! Temos {slots.Count} opção(ões) de {especialidade.Nome}:\n\n" +
                  string.Join("\n", linhas) +
                  "\n\nQual prefere?";

        await _twilioService.SendMessage(numeroWhatsApp, msg);
    }

    private async Task OfereceHorariosPorMedico(int clienteId, string numeroWhatsApp, string? medicoNome)
    {
        if (string.IsNullOrEmpty(medicoNome))
        {
            await _twilioService.SendMessage(numeroWhatsApp, "Qual médico você deseja?");
            return;
        }

        var especialista = await _context.Especialistas
            .FirstOrDefaultAsync(e => e.Nome.ToLower().Contains(medicoNome.ToLower()) && e.Ativo);

        if (especialista == null)
        {
            await _twilioService.SendMessage(numeroWhatsApp, $"Não encontrei o médico '{medicoNome}'. Por favor, verifique o nome.");
            return;
        }

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var horarios = await _context.HorariosConsulta
            .Where(h => h.EspecialistaId == especialista.Id && h.Status == "disponivel" && h.DataConsulta >= hoje)
            .OrderBy(h => h.DataConsulta).ThenBy(h => h.HoraInicio)
            .Take(5)
            .ToListAsync();

        if (horarios.Count == 0)
        {
            await _twilioService.SendMessage(numeroWhatsApp,
                $"Não há horários disponíveis com {especialista.Nome} no momento.");
            return;
        }

        var slots = horarios.Select((h, i) => new SlotContexto
        {
            Index = i + 1,
            SlotId = h.Id,
            EspecialistaNome = especialista.Nome,
            Data = h.DataConsulta.ToString("dd/MM/yyyy"),
            Hora = h.HoraInicio.ToString("HH:mm")
        }).ToList();

        await SalvarContextoSessao(clienteId, slots);

        var linhas = slots.Select(s => $"  {s.Index}. {s.Data} às {s.Hora}");
        var msg = $"Horários disponíveis com {especialista.Nome}:\n\n" +
                  string.Join("\n", linhas) +
                  "\n\nQual prefere?";

        await _twilioService.SendMessage(numeroWhatsApp, msg);
    }

    private async Task SalvarContextoSessao(int clienteId, List<SlotContexto> slots)
    {
        var sessao = await _context.SessoesConversa
            .FirstOrDefaultAsync(s => s.ClienteId == clienteId);

        if (sessao != null)
        {
            sessao.EstadoAtual = "aguardando_escolha";
            sessao.ContextoJson = JsonSerializer.Serialize(new ContextoEscolha { Slots = slots });
            sessao.UltimaMensagemEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private static SlotContexto? ResolverEscolha(string mensagem, List<SlotContexto> slots)
    {
        var msg = mensagem.Trim();

        // Tenta por índice numérico
        if (int.TryParse(msg, out var idx))
            return slots.FirstOrDefault(s => s.Index == idx);

        var msgLower = msg.ToLower();

        // Tenta por nome do médico (palavra com mais de 3 letras)
        var palavras = msgLower.Split(new[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Where(p => p.Length > 3);
        var porNome = slots.FirstOrDefault(s =>
            palavras.Any(p => s.EspecialistaNome.ToLower().Contains(p)));
        if (porNome != null) return porNome;

        // Tenta por data (dd/MM)
        var matchData = System.Text.RegularExpressions.Regex.Match(msgLower, @"\d{1,2}/\d{1,2}");
        if (matchData.Success)
        {
            var partes = matchData.Value.Split('/');
            var dataFormatada = $"{partes[0].PadLeft(2, '0')}/{partes[1].PadLeft(2, '0')}";
            return slots.FirstOrDefault(s => s.Data.StartsWith(dataFormatada));
        }

        return null;
    }

    private class SlotContexto
    {
        public int Index { get; set; }
        public int SlotId { get; set; }
        public string EspecialistaNome { get; set; } = "";
        public string Data { get; set; } = "";
        public string Hora { get; set; } = "";
    }

    private class ContextoEscolha
    {
        public List<SlotContexto> Slots { get; set; } = [];
    }
}
