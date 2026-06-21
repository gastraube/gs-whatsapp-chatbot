using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("duvida")]
public class DuvidaHandler : IIntentHandler
{
    private readonly INotificacaoService _notificacao;
    private readonly IHistoricoMensagemRepository _historicoRepo;

    public DuvidaHandler(INotificacaoService notificacao, IHistoricoMensagemRepository historicoRepo)
    {
        _notificacao = notificacao;
        _historicoRepo = historicoRepo;
    }

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        var resposta = llmResponse.Resposta ?? "Desculpe, não consegui processar sua dúvida. Pode reformular?";
        await _notificacao.EnviarMensagemAsync(numeroWhatsApp, resposta);
        await _historicoRepo.AdicionarAsync(new HistoricoMensagem
        {
            ClienteId = clienteId,
            RemetenteId = "bot",
            Mensagem = resposta,
            Tipo = "texto"
        });
    }
}
