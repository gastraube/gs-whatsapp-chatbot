using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

public interface IIntentHandler
{
    Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse);
}