using gschatbot.api.Data;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        try
        {
            // Extrai dados da resposta LLM
            var dados = llmResponse.Dados;
            var especialidadeNome = dados.ContainsKey("especialidade") ? dados["especialidade"].ToString() : null;
            var dataStr = dados.ContainsKey("data") ? dados["data"].ToString() : null;
            var horaStr = dados.ContainsKey("hora") ? dados["hora"].ToString() : null;

            // Validação
            if (string.IsNullOrEmpty(especialidadeNome) || string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(horaStr))
            {
                await _twilioService.SendMessage(numeroWhatsApp, llmResponse.Resposta);
                return;
            }

            // Busca especialidade
            var especialidade = await _context.Especialidades
                .FirstOrDefaultAsync(e => e.Nome.ToLower().Contains(especialidadeNome.ToLower()));

            if (especialidade == null)
            {
                await _twilioService.SendMessage(numeroWhatsApp,
                    $"Desculpe, não encontrei especialista em '{especialidadeNome}'.");
                return;
            }

            // Busca especialista da especialidade
            var especialista = await _context.Especialistas
                .Include(e => e.Especialidade)
                .FirstOrDefaultAsync(e => e.EspecialidadeId == especialidade.Id && e.Ativo);

            if (especialista == null)
            {
                await _twilioService.SendMessage(numeroWhatsApp,
                    $"Desculpe, não há especialista em '{especialidadeNome}' disponível.");
                return;
            }

            // Parse data e hora
            if (!DateOnly.TryParseExact(dataStr, "dd/MM/yyyy", out var data) ||
                !TimeOnly.TryParseExact(horaStr, "HH:mm", out var hora))
            {
                await _twilioService.SendMessage(numeroWhatsApp,
                    "Formato de data/hora inválido. Use DD/MM/YYYY e HH:MM");
                return;
            }

            // Busca slot disponível
            var slot = await _context.HorariosConsulta
                .FirstOrDefaultAsync(h =>
                    h.EspecialistaId == especialista.Id &&
                    h.DataConsulta == data &&
                    h.HoraInicio == hora &&
                    h.Status == "disponivel");

            if (slot == null)
            {
                await _twilioService.SendMessage(numeroWhatsApp,
                    $"Desculpe, o horário {dataStr} às {horaStr} não está disponível.");
                return;
            }

            // Cria agendamento
            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                EspecialistaId = especialista.Id,
                HorarioConsultaId = slot.Id,
                Status = "confirmado"
            };

            _context.Agendamentos.Add(agendamento);

            // Atualiza slot
            slot.Status = "reservado";
            slot.Agendamento = agendamento;

            await _context.SaveChangesAsync();

            // Salva histórico
            var mensagem = new HistoricoMensagem
            {
                ClienteId = clienteId,
                AgendamentoId = agendamento.Id,
                RemetenteId = "bot",
                Mensagem = llmResponse.Resposta,
                Tipo = "agendamento"
            };

            _context.HistoricoMensagens.Add(mensagem);
            await _context.SaveChangesAsync();

            // Responde ao cliente
            var endereco = await _context.Enderecos.FindAsync(slot.EnderecoId);
            var resposta = $"✅ Agendamento confirmado!\n\n" +
                $"📅 Data: {data:dd/MM/yyyy}\n" +
                $"⏰ Hora: {hora:HH:mm}\n" +
                $"👨‍⚕️ Profissional: Dr(a). {especialista.Nome}\n" +
                $"🏥 Especialidade: {especialidade.Nome}\n" +
                $"📍 Local: {endereco.Rua}, {endereco.Numero} - {endereco.Bairro}, {endereco.Cidade}\n\n" +
                $"Obrigado!";

            await _twilioService.SendMessage(numeroWhatsApp, resposta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AgendamentoHandler] Erro: {ex.Message}");
            await _twilioService.SendMessage(numeroWhatsApp, "Erro ao processar agendamento. Tente novamente.");
        }
    }
}