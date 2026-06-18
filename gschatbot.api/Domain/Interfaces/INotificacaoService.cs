namespace gschatbot.api.Domain.Interfaces;

public interface INotificacaoService
{
    Task EnviarMensagemAsync(string numeroWhatsApp, string mensagem);
}
