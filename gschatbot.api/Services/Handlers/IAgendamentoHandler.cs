using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

public interface IAgendamentoHandler : IIntentHandler
{
    Task ProcessarEscolhaAsync(int clienteId, string numeroWhatsApp, string mensagem, SessaoConversa sessao);
}
