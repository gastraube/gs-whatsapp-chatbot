using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("duvida")]
public class DuvidaHandler : IIntentHandler
{
    private readonly INotificacaoService _notificacao;

    public DuvidaHandler(INotificacaoService notificacao)
    {
        _notificacao = notificacao;
    }

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        var resposta = llmResponse.Resposta ?? "Desculpe, não consegui processar sua dúvida. Pode reformular?";
        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, resposta);
    }
}
