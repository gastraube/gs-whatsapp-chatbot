using gschatbot.api.Configuration;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace gschatbot.api.Infrastructure.Services;

public class OllamaLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaLlmService> _logger;
    private readonly string _promptTemplate;

    public OllamaLlmService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        var path = Path.Combine(AppContext.BaseDirectory, "Prompts", "chatbot_prompt.txt");
        _promptTemplate = File.ReadAllText(path);
    }

    public async Task<LlmResponse> ProcessarMensagemAsync(
        string mensagem,
        List<(string role, string texto)>? historico = null,
        List<string>? especialidades = null,
        List<string>? especialistas = null,
        List<string>? planos = null,
        List<string>? planosCliente = null,
        Dictionary<string, List<string>>? metodosPagamentoPorEspecialista = null)
    {
        try
        {
            var prompt = BuildPrompt(mensagem, historico, especialidades, especialistas, planos, planosCliente, metodosPagamentoPorEspecialista);
            var json = await CallOllamaAsync(prompt);
            return ParseResponse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OllamaLlmService] Erro ao processar mensagem");
            return new LlmResponse { Intent = "erro", Resposta = "Erro ao processar mensagem." };
        }
    }

    private string BuildPrompt(
        string mensagem,
        List<(string role, string texto)>? historico,
        List<string>? especialidades,
        List<string>? especialistas,
        List<string>? planos,
        List<string>? planosCliente,
        Dictionary<string, List<string>>? metodosPagamentoPorEspecialista)
    {
        var historicoBloco = historico?.Count > 0
            ? "\n--- Histórico recente ---\n" +
              string.Join("\n", historico.Select(h => $"{h.role}: {h.texto}")) +
              "\n--- Fim ---"
            : "";

        var especialidadesBloco = especialidades?.Count > 0
            ? $"Especialidades disponíveis (use exatamente este nome): {string.Join(", ", especialidades)}"
            : "";

        var especialistasBloco = especialistas?.Count > 0
            ? $"Médicos disponíveis (use exatamente este nome): {string.Join(", ", especialistas)}"
            : "";

        var planosBloco = planos?.Count > 0
            ? $"Planos de saúde aceitos na clínica: {string.Join(", ", planos)}"
            : "";

        var planosClienteBloco = planosCliente?.Count > 0
            ? $"Planos de saúde do cliente: {string.Join(", ", planosCliente)}"
            : "";

        var metodosPagamentoBloco = metodosPagamentoPorEspecialista?.Count > 0
            ? "Métodos de pagamento por médico:\n" +
              string.Join("\n", metodosPagamentoPorEspecialista.Select(kv =>
                  $"  {kv.Key}: {string.Join(", ", kv.Value)}"))
            : "";

        return _promptTemplate
            .Replace("{{ESPECIALIDADES}}", especialidadesBloco)
            .Replace("{{ESPECIALISTAS}}", especialistasBloco)
            .Replace("{{PLANOS}}", planosBloco)
            .Replace("{{PLANOS_CLIENTE}}", planosClienteBloco)
            .Replace("{{METODOS_PAGAMENTO}}", metodosPagamentoBloco)
            .Replace("{{HISTORICO}}", historicoBloco)
            .Replace("{{MENSAGEM}}", mensagem);
    }

    private async Task<string> CallOllamaAsync(string prompt)
    {
        var body = JsonSerializer.Serialize(new
        {
            model = _options.Model,
            prompt,
            stream = false,
            format = "json"
        });

        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_options.Url}/api/generate", content);
        var text = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.GetProperty("response").GetString() ?? "";
    }

    private static LlmResponse ParseResponse(string raw)
    {
        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}') + 1;

        if (start < 0 || end <= start)
            return new LlmResponse { Intent = "duvida", Resposta = "Desculpe, houve um erro ao processar." };

        var jsonStr = raw[start..end];

        JsonElement root;
        try
        {
            root = JsonDocument.Parse(jsonStr).RootElement;
        }
        catch (JsonException)
        {
            var cleaned = jsonStr
                .Replace("\\\"", "\"")
                .Replace("\\r\\n", "")
                .Replace("\\n", "")
                .Replace("\\t", " ");
            root = JsonDocument.Parse(cleaned).RootElement;
        }

        var intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "duvida" : "duvida";
        var resposta = root.TryGetProperty("resposta", out var r) ? r.GetString() ?? "Desculpe, não entendi." : "Desculpe, não entendi.";

        var dados = root.TryGetProperty("dados", out var d) && d.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(d.GetRawText()) ?? []
            : [];

        return new LlmResponse { Intent = intent, Resposta = resposta, Dados = dados };
    }
}
