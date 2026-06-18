using gschatbot.api.Application.Agendamento;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("agendar")]
public class AgendamentoHandler : IAgendamentoHandler
{
    private const int PageSize = 5;

    private readonly IHorarioConsultaRepository _horarioRepo;
    private readonly IEspecialistaRepository _especialistaRepo;
    private readonly IEspecialidadeRepository _especialidadeRepo;
    private readonly IAgendamentoRepository _agendamentoRepo;
    private readonly IHistoricoMensagemRepository _historicoRepo;
    private readonly ISessaoConversaRepository _sessaoRepo;
    private readonly INotificacaoService _notificacao;
    private readonly ILogger<AgendamentoHandler> _logger;

    public AgendamentoHandler(
        IHorarioConsultaRepository horarioRepo,
        IEspecialistaRepository especialistaRepo,
        IEspecialidadeRepository especialidadeRepo,
        IAgendamentoRepository agendamentoRepo,
        IHistoricoMensagemRepository historicoRepo,
        ISessaoConversaRepository sessaoRepo,
        INotificacaoService notificacao,
        ILogger<AgendamentoHandler> logger)
    {
        _horarioRepo = horarioRepo;
        _especialistaRepo = especialistaRepo;
        _especialidadeRepo = especialidadeRepo;
        _agendamentoRepo = agendamentoRepo;
        _historicoRepo = historicoRepo;
        _sessaoRepo = sessaoRepo;
        _notificacao = notificacao;
        _logger = logger;
    }

    // Round 1: detecta tipo e oferece lista de horários
    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        try
        {
            var dados = llmResponse.Dados;
            var tipo = InferirTipo(dados);
            var medicoNome = dados.ContainsKey("medico") ? dados["medico"]?.ToString() : null;
            var especialidadeNome = dados.ContainsKey("especialidade") ? dados["especialidade"]?.ToString() : null;

            if (tipo == "especialidade")
            {
                await OfereceHorariosPorEspecialidadeAsync(clienteId, numeroWhatsApp, especialidadeNome);
                return;
            }

            if (tipo == "medico")
            {
                await OfereceHorariosPorMedicoAsync(clienteId, numeroWhatsApp, medicoNome);
                return;
            }

            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, llmResponse.Resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AgendamentoHandler] Erro ao processar intent agendar");
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Erro ao processar agendamento. Tente novamente.");
        }
    }

    // Round 2: cliente escolheu da lista, cria agendamento
    public async Task ProcessarEscolhaAsync(int clienteId, string numeroWhatsApp, string mensagem, SessaoConversa sessao)
    {
        try
        {
            var ctx = DeserializarContexto(sessao.ContextoJson);

            if (ctx?.Slots == null || ctx.Slots.Count == 0)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Desculpe, perdi o contexto. Por favor, solicite o agendamento novamente.");
                await ResetarSessaoAsync(sessao);
                return;
            }

            if (PedindoMaisHorarios(mensagem) && !string.IsNullOrEmpty(ctx.TipoBusca))
            {
                await BuscarProximaPaginaAsync(clienteId, numeroWhatsApp, ctx);
                return;
            }

            var escolhido = ResolverEscolha(mensagem, ctx.Slots);

            if (escolhido == null)
            {
                var linhas = ctx.Slots.Select(s => $"  {s.Index}. {s.EspecialistaNome} - {s.Data} às {s.Hora}");
                var reenvio = "Desculpe, não entendi. Digite o número da opção desejada:\n\n" +
                              string.Join("\n", linhas);
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, reenvio);
                return;
            }

            await ConfirmarAgendamentoAsync(clienteId, numeroWhatsApp, escolhido.SlotId, sessao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AgendamentoHandler.ProcessarEscolhaAsync] Erro ao processar escolha");
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Erro ao confirmar agendamento. Tente novamente.");
        }
    }

    private async Task OfereceHorariosPorEspecialidadeAsync(int clienteId, string numeroWhatsApp, string? especialidadeNome, int offset = 0)
    {
        if (string.IsNullOrEmpty(especialidadeNome))
        {
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Qual especialidade você deseja?");
            return;
        }

        var especialidade = await _especialidadeRepo.BuscarPorNomeAsync(especialidadeNome);

        if (especialidade == null)
        {
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, $"Não encontrei a especialidade '{especialidadeNome}'. Por favor, verifique o nome.");
            return;
        }

        var pagina = await _horarioRepo.ListarPorEspecialidadeAsync(especialidade.Id, offset, PageSize + 1);
        var temMais = pagina.Count > PageSize;
        var itens = pagina.Take(PageSize).ToList();

        if (itens.Count == 0)
        {
            var semHorario = offset == 0
                ? $"Não há horários disponíveis para {especialidade.Nome} no momento."
                : $"Não há mais horários disponíveis para {especialidade.Nome}.";
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, semHorario);
            return;
        }

        var slots = itens.Select((x, i) => new SlotContexto
        {
            Index = i + 1,
            SlotId = x.SlotId,
            EspecialistaNome = x.EspecialistaNome,
            Data = x.DataConsulta.ToString("dd/MM/yyyy"),
            Hora = x.HoraInicio.ToString("HH:mm")
        }).ToList();

        await SalvarContextoAsync(clienteId, slots, "especialidade", especialidade.Nome, offset + PageSize);

        var linhas = slots.Select(s => $"  {s.Index}. {s.EspecialistaNome} - {s.Data} às {s.Hora}");
        var rodape = temMais
            ? "\n\nQual prefere? (ou diga \"mais\" para ver outros horários)"
            : "\n\nQual prefere?";
        var cabecalho = offset == 0
            ? $"Horários disponíveis de {especialidade.Nome}:\n\n"
            : $"Próximos horários de {especialidade.Nome}:\n\n";

        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, cabecalho + string.Join("\n", linhas) + rodape);
    }

    private async Task OfereceHorariosPorMedicoAsync(int clienteId, string numeroWhatsApp, string? medicoNome, int offset = 0)
    {
        if (string.IsNullOrEmpty(medicoNome))
        {
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Qual médico você deseja?");
            return;
        }

        var especialista = await _especialistaRepo.BuscarPorNomeAsync(medicoNome);

        if (especialista == null)
        {
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, $"Não encontrei nenhum médico chamado '{medicoNome}' em nossa clínica. Verifique o nome ou me diga a especialidade desejada.");
            return;
        }

        var pagina = await _horarioRepo.ListarPorEspecialistaAsync(especialista.Id, offset, PageSize + 1);
        var temMais = pagina.Count > PageSize;
        var itens = pagina.Take(PageSize).ToList();

        if (itens.Count == 0)
        {
            var semHorario = offset == 0
                ? $"Não há horários disponíveis com {especialista.Nome} no momento."
                : $"Não há mais horários disponíveis com {especialista.Nome}.";
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, semHorario);
            return;
        }

        var slots = itens.Select((x, i) => new SlotContexto
        {
            Index = i + 1,
            SlotId = x.SlotId,
            EspecialistaNome = x.EspecialistaNome,
            Data = x.DataConsulta.ToString("dd/MM/yyyy"),
            Hora = x.HoraInicio.ToString("HH:mm")
        }).ToList();

        await SalvarContextoAsync(clienteId, slots, "medico", especialista.Nome, offset + PageSize);

        var linhas = slots.Select(s => $"  {s.Index}. {s.Data} às {s.Hora}");
        var rodape = temMais
            ? "\n\nQual prefere? (ou diga \"mais\" para ver outros horários)"
            : "\n\nQual prefere?";
        var cabecalho = offset == 0
            ? $"Horários disponíveis com {especialista.Nome}:\n\n"
            : $"Próximos horários com {especialista.Nome}:\n\n";

        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, cabecalho + string.Join("\n", linhas) + rodape);
    }

    private async Task ConfirmarAgendamentoAsync(int clienteId, string numeroWhatsApp, int slotId, SessaoConversa sessao)
    {
        var slot = await _horarioRepo.BuscarDisponivelComDetalhesAsync(slotId);

        if (slot == null)
        {
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp,
                "Desculpe, esse horário acabou de ser ocupado. Solicite novamente para ver horários atualizados.");
            await ResetarSessaoAsync(sessao);
            return;
        }

        var agendamento = new Agendamento
        {
            ClienteId = clienteId,
            EspecialistaId = slot.EspecialistaId,
            HorarioConsultaId = slot.Id,
            Status = "confirmado"
        };

        await _agendamentoRepo.CriarAsync(agendamento);
        await _horarioRepo.ReservarAsync(slot);

        sessao.EstadoAtual = "inicial";
        sessao.ContextoJson = "{}";
        sessao.UltimaMensagemEm = DateTime.UtcNow;
        await _sessaoRepo.AtualizarAsync(sessao);

        var resposta = $"✅ Agendamento confirmado!\n\n" +
            $"📅 Data: {slot.DataConsulta:dd/MM/yyyy}\n" +
            $"⏰ Hora: {slot.HoraInicio:HH:mm}\n" +
            $"👨‍⚕️ Profissional: {slot.Especialista.Nome}\n" +
            $"🏥 Especialidade: {slot.Especialista.Especialidade.Nome}\n" +
            $"📍 Local: {slot.Endereco.Rua}, {slot.Endereco.Numero} - {slot.Endereco.Bairro}, {slot.Endereco.Cidade}\n\n" +
            $"Obrigado!";

        await _historicoRepo.AdicionarAsync(new HistoricoMensagem
        {
            ClienteId = clienteId,
            AgendamentoId = agendamento.Id,
            RemetenteId = "bot",
            Mensagem = resposta,
            Tipo = "agendamento"
        });

        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, resposta);
    }

    private async Task BuscarProximaPaginaAsync(int clienteId, string numeroWhatsApp, ContextoEscolha ctx)
    {
        if (ctx.TipoBusca == "especialidade")
        {
            await OfereceHorariosPorEspecialidadeAsync(clienteId, numeroWhatsApp, ctx.NomeBusca, ctx.Offset);
            return;
        }

        await OfereceHorariosPorMedicoAsync(clienteId, numeroWhatsApp, ctx.NomeBusca, ctx.Offset);
    }

    private async Task SalvarContextoAsync(int clienteId, List<SlotContexto> slots, string tipoBusca, string nomeBusca, int proximoOffset)
    {
        var sessao = await _sessaoRepo.BuscarPorClienteAsync(clienteId);

        if (sessao == null)
            return;

        sessao.EstadoAtual = "aguardando_escolha";
        sessao.ContextoJson = JsonSerializer.Serialize(new ContextoEscolha
        {
            Slots = slots,
            TipoBusca = tipoBusca,
            NomeBusca = nomeBusca,
            Offset = proximoOffset
        });
        sessao.UltimaMensagemEm = DateTime.UtcNow;

        await _sessaoRepo.AtualizarAsync(sessao);
    }

    private async Task ResetarSessaoAsync(SessaoConversa sessao)
    {
        sessao.EstadoAtual = "inicial";
        sessao.ContextoJson = "{}";
        await _sessaoRepo.AtualizarAsync(sessao);
    }

    private static string? InferirTipo(Dictionary<string, object> dados)
    {
        var tipo = dados.ContainsKey("tipo") ? dados["tipo"]?.ToString() : null;

        if (!string.IsNullOrEmpty(tipo))
            return tipo;

        var medicoNome = dados.ContainsKey("medico") ? dados["medico"]?.ToString() : null;
        if (!string.IsNullOrEmpty(medicoNome))
            return "medico";

        var especialidadeNome = dados.ContainsKey("especialidade") ? dados["especialidade"]?.ToString() : null;
        if (!string.IsNullOrEmpty(especialidadeNome))
            return "especialidade";

        return null;
    }

    private static bool PedindoMaisHorarios(string mensagem)
    {
        var lower = mensagem.ToLower();
        return lower.Contains("mais") || lower.Contains("outro") ||
               lower.Contains("próximo") || lower.Contains("proximo") ||
               lower.Contains("só tem") || lower.Contains("so tem") ||
               lower.Contains("tem mais") || lower.Contains("ver mais");
    }

    private static ContextoEscolha? DeserializarContexto(string contextojson)
    {
        try
        {
            return JsonSerializer.Deserialize<ContextoEscolha>(contextojson);
        }
        catch
        {
            return null;
        }
    }

    private static SlotContexto? ResolverEscolha(string mensagem, List<SlotContexto> slots)
    {
        var msg = mensagem.Trim();

        if (int.TryParse(msg, out var idx))
            return slots.FirstOrDefault(s => s.Index == idx);

        var msgLower = msg.ToLower();

        var palavras = msgLower.Split(new[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries)
                               .Where(p => p.Length > 3);

        var porNome = slots.FirstOrDefault(s =>
            palavras.Any(p => s.EspecialistaNome.ToLower().Contains(p)));

        if (porNome != null)
            return porNome;

        var matchData = Regex.Match(msgLower, @"\d{1,2}/\d{1,2}");

        if (!matchData.Success)
            return null;

        var partes = matchData.Value.Split('/');
        var dataFormatada = $"{partes[0].PadLeft(2, '0')}/{partes[1].PadLeft(2, '0')}";
        return slots.FirstOrDefault(s => s.Data.StartsWith(dataFormatada));
    }
}
