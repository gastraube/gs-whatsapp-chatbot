namespace gschatbot.api.Services.Handlers;

[AttributeUsage(AttributeTargets.Class)]
public class IntentHandlerAttribute : Attribute
{
    public string Intent { get; }

    public IntentHandlerAttribute(string intent)
    {
        Intent = intent;
    }
}