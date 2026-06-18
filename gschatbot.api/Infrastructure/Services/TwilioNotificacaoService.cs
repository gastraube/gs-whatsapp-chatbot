using gschatbot.api.Configuration;
using gschatbot.api.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace gschatbot.api.Infrastructure.Services;

public class TwilioNotificacaoService : INotificacaoService
{
    private readonly TwilioOptions _options;
    private readonly ILogger<TwilioNotificacaoService> _logger;

    public TwilioNotificacaoService(
        IOptions<TwilioOptions> options,
        ILogger<TwilioNotificacaoService> logger)
    {
        _options = options.Value;
        _logger = logger;

        TwilioClient.Init(_options.AccountSid, _options.AuthToken);
    }

    public async Task EnviarMensagemAsync(string numeroWhatsApp, string mensagem)
    {
        try
        {
            var from = _options.PhoneNumber.StartsWith("whatsapp:")
                ? _options.PhoneNumber
                : $"whatsapp:{_options.PhoneNumber}";

            var to = $"whatsapp:+{numeroWhatsApp.Replace("whatsapp:", "").TrimStart('+')}";

            var message = await MessageResource.CreateAsync(
                body: mensagem,
                from: new PhoneNumber(from),
                to: new PhoneNumber(to)
            );

            _logger.LogInformation("[TwilioNotificacaoService] Mensagem enviada: {Sid}", message.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TwilioNotificacaoService] Erro ao enviar mensagem para {Numero}", numeroWhatsApp);
            throw;
        }
    }
}
