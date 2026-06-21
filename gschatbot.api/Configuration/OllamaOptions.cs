namespace gschatbot.api.Configuration;

public class OllamaOptions
{
    public const string Section = "Ollama";

    public string Url { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "qwen2.5:7b";
}
