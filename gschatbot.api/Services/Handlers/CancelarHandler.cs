using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using System.Globalization;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("cancelar")]
public class CancelarHandler : IIntentHandler
{
    private readonly IAgendamentoRepository _agendamentoRepo;
    private readonly IEspecialistaRepository _especialistaRepo;
    private readonly INotificacaoService _notificacao;
    private readonly ILogger<CancelarHandler> _logger;

    public CancelarHandler(
        IAgendamentoRepository agendamentoRepo,
        IEspecialistaRepository especialistaRepo,
        INotificacaoService notificacao,
        ILogger<CancelarHandler> logger)
    {
        _agendamentoRepo = agendamentoRepo;
        _especialistaRepo = especialistaRepo;
        _notificacao = notificacao;
        _logger = logger;
    }

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        try
        {
            var dados = llmResponse.Dados;
            var medicoNome = dados.TryGetValue("medico", out var m) ? m?.ToString() : null;
            var dataStr = dados.TryGetValue("data", out var d) ? d?.ToString() : null;
            var horaStr = dados.TryGetValue("hora", out var h) ? h?.ToString() : null;
            var confirmado = dados.TryGetValue("confirmado", out var c) && c?.ToString()?.ToLower() == "true";

            // Sem dados específicos: lista consultas futuras
            if (string.IsNullOrEmpty(medicoNome) || string.IsNullOrEmpty(dataStr))
            {
                var agendamentos = await _agendamentoRepo.ListarFuturosAsync(clienteId);

                if (agendamentos.Count == 0)
                {
                    await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Você não tem consultas agendadas.");
                    return;
                }

                var linhas = agendamentos.Select((a, i) =>
                    $"  {i + 1}. {a.Especialista.Nome} - {a.HorarioConsulta.DataConsulta:dd/MM/yyyy} às {a.HorarioConsulta.HoraInicio:HH:mm}");
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp,
                    $"Suas consultas agendadas:\n\n{string.Join("\n", linhas)}\n\nQual deseja cancelar?");
                return;
            }

            // Tem dados do médico/data: resolve o agendamento
            var especialista = await _especialistaRepo.BuscarPorNomeAsync(medicoNome);
            if (especialista == null)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Não encontrei essa consulta. Pode tentar novamente?");
                return;
            }

            var futuros = await _agendamentoRepo.ListarFuturosAsync(clienteId);
            Agendamento? alvo = null;

            if (!string.IsNullOrEmpty(dataStr) &&
                DateOnly.TryParseExact(dataStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            {
                alvo = futuros.FirstOrDefault(a =>
                    a.EspecialistaId == especialista.Id && a.HorarioConsulta.DataConsulta == data);
            }
            else
            {
                alvo = futuros.FirstOrDefault(a => a.EspecialistaId == especialista.Id);
            }

            if (alvo == null)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Não encontrei essa consulta nos seus agendamentos futuros.");
                return;
            }

            // Pede confirmação antes de cancelar
            if (!confirmado)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp,
                    $"Tem certeza que deseja cancelar a consulta com {alvo.Especialista.Nome} em {alvo.HorarioConsulta.DataConsulta:dd/MM/yyyy} às {alvo.HorarioConsulta.HoraInicio:HH:mm}?");
                return;
            }

            await _agendamentoRepo.CancelarAsync(alvo.Id);
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp,
                $"Consulta com {alvo.Especialista.Nome} em {alvo.HorarioConsulta.DataConsulta:dd/MM/yyyy} cancelada. O horário foi liberado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CancelarHandler] Erro ao cancelar agendamento");
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Erro ao cancelar o agendamento. Tente novamente.");
        }
    }
}
