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

    public OllamaLlmService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaLlmService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<LlmResponse> ProcessarMensagemAsync(
        string mensagem,
        List<(string role, string texto)>? historico = null,
        List<string>? especialidades = null,
        List<string>? especialistas = null)
    {
        try
        {
            var prompt = BuildPrompt(mensagem, historico, especialidades, especialistas);
            var json = await CallOllamaAsync(prompt);
            return ParseResponse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OllamaLlmService] Erro ao processar mensagem");
            return new LlmResponse { Intent = "erro", Resposta = "Erro ao processar mensagem." };
        }
    }

    private static string BuildPrompt(
        string mensagem,
        List<(string role, string texto)>? historico,
        List<string>? especialidades,
        List<string>? especialistas)
    {
        var historicoBloco = historico?.Count > 0
            ? "\n\n--- Histórico recente ---\n" +
              string.Join("\n", historico.Select(h => $"{h.role}: {h.texto}")) +
              "\n--- Fim ---\n"
            : "";

        var especialidadesBloco = especialidades?.Count > 0
            ? $"\nEspecialidades disponíveis (use exatamente este nome): {string.Join(", ", especialidades)}"
            : "";

        var especialistasBloco = especialistas?.Count > 0
            ? $"\nMédicos disponíveis (use exatamente este nome): {string.Join(", ", especialistas)}"
            : "";

        return $@"Você é um assistente de agendamento de clínica médica.

        Instruções:
        - Quando o cliente quiser agendar, descubra se ele prefere uma especialidade ou um médico específico.
        - NUNCA pergunte data ou hora — o sistema busca os horários disponíveis automaticamente.
        - Use o histórico para não repetir perguntas já respondidas.
        - Só defina intent=""agendar"" quando souber o tipo (especialidade ou medico) E o nome.
        - Se não souber o tipo/nome, pergunte e use intent=""agendar"" com dados.tipo=null.
        - ""Dr X"", ""doutor X"", ""doutora X"", ""Dr. X"" sempre significa tipo=""medico"" com medico=""X"" (sem o prefixo Dr/doutor). Defina tipo=""medico"" mesmo que o nome não conste na lista de médicos disponíveis.
        {especialidadesBloco}
        {especialistasBloco}
        {historicoBloco}
        Responda APENAS com JSON válido:
        {{""intent"": ""agendar""|""duvida""|""saudacao""|""cancelar"", ""resposta"": ""..."", ""dados"": {{""tipo"": ""especialidade""|""medico""|null, ""especialidade"": ""...""|null, ""medico"": ""...""|null}}}}

        Mensagem do cliente: {mensagem}";
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
