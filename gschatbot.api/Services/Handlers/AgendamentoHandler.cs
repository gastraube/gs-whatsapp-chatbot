using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("agendar")]
public class AgendamentoHandler : IIntentHandler
{
    private const int PageSize = 5;

    private readonly IHorarioConsultaRepository _horarioRepo;
    private readonly IEspecialistaRepository _especialistaRepo;
    private readonly IEspecialidadeRepository _especialidadeRepo;
    private readonly INotificacaoService _notificacao;
    private readonly IHistoricoMensagemRepository _historicoRepo;
    private readonly ILogger<AgendamentoHandler> _logger;

    public AgendamentoHandler(
        IHorarioConsultaRepository horarioRepo,
        IEspecialistaRepository especialistaRepo,
        IEspecialidadeRepository especialidadeRepo,
        INotificacaoService notificacao,
        IHistoricoMensagemRepository historicoRepo,
        ILogger<AgendamentoHandler> logger)
    {
        _horarioRepo = horarioRepo;
        _especialistaRepo = especialistaRepo;
        _especialidadeRepo = especialidadeRepo;
        _notificacao = notificacao;
        _historicoRepo = historicoRepo;
        _logger = logger;
    }

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
                await ListarEspecialistasAsync(clienteId, numeroWhatsApp, especialidadeNome);
                return;
            }

            if (tipo == "medico")
            {
                await ListarHorariosMedicoAsync(clienteId, numeroWhatsApp, medicoNome);
                return;
            }

            var resposta = string.IsNullOrWhiteSpace(llmResponse.Resposta)
                ? "Diga o médico ou a especialidade que deseja para agendar uma consulta."
                : llmResponse.Resposta;
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AgendamentoHandler] Erro ao processar intent agendar");
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Erro ao processar agendamento. Tente novamente.");
        }
    }

    private async Task ListarEspecialistasAsync(int clienteId, string numeroWhatsApp, string? especialidadeNome)
    {
        if (string.IsNullOrEmpty(especialidadeNome))
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, "Qual especialidade você deseja?");
            return;
        }

        var especialidade = await _especialidadeRepo.BuscarPorNomeAsync(especialidadeNome);
        if (especialidade == null)
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, $"Não encontrei a especialidade '{especialidadeNome}'. Pode verificar o nome?");
            return;
        }

        var especialistas = await _especialistaRepo.ListarPorEspecialidadeAsync(especialidade.Id);
        if (especialistas.Count == 0)
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, $"Não há médicos disponíveis para {especialidade.Nome} no momento.");
            return;
        }

        var linhas = especialistas.Select(e => $"  • {e.Nome}");
        var mensagem = $"Médicos disponíveis em {especialidade.Nome}:\n\n{string.Join("\n", linhas)}\n\nQual prefere?";
        await EnviarESalvarAsync(clienteId, numeroWhatsApp, mensagem);
    }

    private async Task ListarHorariosMedicoAsync(int clienteId, string numeroWhatsApp, string? medicoNome)
    {
        if (string.IsNullOrEmpty(medicoNome))
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, "Qual médico você deseja?");
            return;
        }

        var especialista = await _especialistaRepo.BuscarPorNomeAsync(medicoNome);
        if (especialista == null)
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, $"Não encontrei nenhum médico chamado '{medicoNome}'. Verifique o nome ou me diga a especialidade desejada.");
            return;
        }

        var slots = await _horarioRepo.ListarPorEspecialistaAsync(especialista.Id, PageSize);
        if (slots.Count == 0)
        {
            await EnviarESalvarAsync(clienteId, numeroWhatsApp, $"Não há horários disponíveis com {especialista.Nome} no momento.");
            return;
        }

        var linhas = slots.Select(x => $"  • {x.DataConsulta:dd/MM/yyyy} às {x.HoraInicio:HH:mm}h");
        var mensagem = $"Horários disponíveis com {especialista.Nome}:\n\n{string.Join("\n", linhas)}\n\nQual prefere?";
        await EnviarESalvarAsync(clienteId, numeroWhatsApp, mensagem);
    }

    private async Task EnviarESalvarAsync(int clienteId, string numeroWhatsApp, string mensagem)
    {
        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, mensagem);
        await _historicoRepo.AdicionarAsync(new HistoricoMensagem
        {
            ClienteId = clienteId,
            RemetenteId = "bot",
            Mensagem = mensagem,
            Tipo = "texto"
        });
    }

    private static string? InferirTipo(Dictionary<string, object> dados)
    {
        if (dados.TryGetValue("tipo", out var tipo) && !string.IsNullOrEmpty(tipo?.ToString()))
            return tipo.ToString();

        if (dados.TryGetValue("medico", out var medico) && !string.IsNullOrEmpty(medico?.ToString()))
            return "medico";

        if (dados.TryGetValue("especialidade", out var especialidade) && !string.IsNullOrEmpty(especialidade?.ToString()))
            return "especialidade";

        return null;
    }
}
