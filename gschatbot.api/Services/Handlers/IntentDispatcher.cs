using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

public class IntentDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IntentDispatcher> _logger;
    private readonly Dictionary<string, Type> _handlers = new();

    public IntentDispatcher(IServiceProvider serviceProvider, ILogger<IntentDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        var handlerType = typeof(IIntentHandler);
        var handlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => handlerType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

        foreach (var handler in handlers)
        {
            var attribute = (IntentHandlerAttribute?)Attribute.GetCustomAttribute(handler, typeof(IntentHandlerAttribute));

            if (attribute == null)
                continue;

            var key = attribute.Intent.ToLower().Trim();
            _handlers[key] = handler;
            _logger.LogInformation("[IntentDispatcher] Registrado: {Key} -> {Handler}", key, handler.Name);
        }
    }

    public async Task<bool> Dispatch(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        if (llmResponse?.Intent == null)
        {
            _logger.LogWarning("[IntentDispatcher] Intent nulo");
            return false;
        }

        var key = llmResponse.Intent.ToLower().Trim();

        if (!_handlers.TryGetValue(key, out var handlerType))
        {
            _logger.LogWarning("[IntentDispatcher] Sem handler para: {Key}", key);
            return false;
        }

        try
        {
            var handler = (IIntentHandler)_serviceProvider.GetRequiredService(handlerType);
            await handler.Handle(clienteId, numeroWhatsApp, llmResponse);
            _logger.LogInformation("[IntentDispatcher] Executado: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[IntentDispatcher] Erro ao executar {Key}", key);
            return false;
        }
    }
}
