namespace gschatbot.api.Models;

public class LlmResponse
{
    public string Intent { get; set; }
    public string Resposta { get; set; }
    public Dictionary<string, object> Dados { get; set; } = new();
}