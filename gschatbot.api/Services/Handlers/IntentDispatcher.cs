using gschatbot.api.Models;

namespace gschatbot.api.Services.Handlers;

public class IntentDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _handlers = new();

    public IntentDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        var handlerType = typeof(IIntentHandler);
        var handlers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => handlerType.IsAssignableFrom(p) && !p.IsInterface);

        foreach (var handler in handlers)
        {
            var attribute = (IntentHandlerAttribute)Attribute.GetCustomAttribute(handler, typeof(IntentHandlerAttribute));
            if (attribute != null)
            {
                var key = attribute.Intent.ToLower().Trim();
                _handlers[key] = handler;
                Console.WriteLine($"[IntentDispatcher] Registrado: {key} -> {handler.Name}");
            }
        }
    }

    public async Task Dispatch(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        if (llmResponse?.Intent == null)
        {
            Console.WriteLine("[IntentDispatcher] Intent nulo");
            return;
        }

        var key = llmResponse.Intent.ToLower().Trim();

        if (_handlers.TryGetValue(key, out var handlerType))
        {
            try
            {
                var handler = (IIntentHandler)_serviceProvider.GetService(handlerType);
                await handler.Handle(clienteId, numeroWhatsApp, llmResponse);
                Console.WriteLine($"[IntentDispatcher] Executado: {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IntentDispatcher] Erro ao executar {key}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[IntentDispatcher] Intent não encontrado: {key}");
        }
    }
}