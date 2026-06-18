using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("duvida")]
public class DuvidaHandler : IIntentHandler
{
    private readonly TwilioService _twilioService;

    public DuvidaHandler(TwilioService twilioService)
    {
        _twilioService = twilioService;
    }

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        var resposta = llmResponse.Resposta ?? "Desculpe, não consegui processar sua dúvida. Pode reformular?";
        await _twilioService.SendMessage(numeroWhatsApp, resposta);
    }
}
