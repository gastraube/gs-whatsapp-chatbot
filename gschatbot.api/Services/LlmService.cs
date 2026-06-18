using gschatbot.api.Models;
using System.Text.Json;

namespace gschatbot.api.Services;

public class LlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public LlmService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<LlmResponse> ProcessMessage(string userMessage, List<(string role, string message)>? historico = null, List<string>? especialidades = null)
    {
        try
        {
            var ollamaUrl = _config["Ollama:Url"] ?? "http://localhost:11434";
            var model = _config["Ollama:Model"] ?? "mistral";

            var historicoTexto = "";
            if (historico != null && historico.Count > 0)
            {
                historicoTexto = "\n\n--- Histórico recente desta conversa com o cliente ---\n" +
                    string.Join("\n", historico.Select(h => $"{h.role}: {h.message}")) +
                    "\n--- Fim do histórico ---\n";
            }

            var especialidadesTexto = especialidades != null && especialidades.Count > 0
                ? $"\nEspecialidades disponíveis na clínica (use exatamente este nome no campo especialidade): {string.Join(", ", especialidades)}\n"
                : "";

            var prompt = $@"Você é um assistente de agendamento de clínica médica.

Instruções:
- Se o cliente quiser agendar mas não informar todos os dados (especialidade, data e hora), pergunte o que falta de forma conversacional.
- Use o histórico abaixo para entender o contexto da conversa e não repetir perguntas já respondidas.
- Só defina intent ""agendar"" quando os 3 dados estiverem presentes: especialidade, data e hora.
- Para especialidade, use EXATAMENTE um dos nomes listados abaixo.
{especialidadesTexto}{historicoTexto}
Responda APENAS com um objeto JSON válido contendo exatamente estes 3 campos:

1. ""intent"": uma das opções: ""agendar"", ""duvida"", ""saudacao"" ou ""cancelar""
2. ""resposta"": texto em português para enviar ao cliente
3. ""dados"": objeto com ""especialidade"", ""data"" (DD/MM/YYYY) e ""hora"" (HH:MM), ou null se não informado

Exemplo de saída:
{{""intent"":""agendar"",""resposta"":""Agendamento confirmado!"",""dados"":{{""especialidade"":""cardiologista"",""data"":""25/06/2026"",""hora"":""14:00""}}}}

Mensagem atual do cliente: {userMessage}";

            var requestBody = new
            {
                model = model,
                prompt = prompt,
                stream = false,
                format = "json"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{ollamaUrl}/api/generate", content);
            var responseText = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseText);
            var responseContent = doc.RootElement.GetProperty("response").GetString();

            // Extrai JSON da resposta
            var jsonStart = responseContent.IndexOf('{');
            var jsonEnd = responseContent.LastIndexOf('}') + 1;

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonString = responseContent.Substring(jsonStart, jsonEnd - jsonStart);

                JsonDocument jsonDoc;
                try
                {
                    jsonDoc = JsonDocument.Parse(jsonString);
                }
                catch (JsonException)
                {
                    // Mistral às vezes gera JSON duplamente escapado: \" → ", \r\n → remove
                    var cleaned = jsonString
                        .Replace("\\\"", "\"")
                        .Replace("\\r\\n", "")
                        .Replace("\\r", "")
                        .Replace("\\n", "")
                        .Replace("\\t", " ");
                    jsonDoc = JsonDocument.Parse(cleaned);
                }

                var root = jsonDoc.RootElement;

                var intent = root.TryGetProperty("intent", out var intentEl)
                    ? intentEl.GetString() ?? "duvida"
                    : "duvida";

                var resposta = root.TryGetProperty("resposta", out var respostaEl)
                    ? respostaEl.GetString() ?? "Desculpe, não entendi."
                    : "Desculpe, não entendi.";

                // Aceita tanto {"dados": {...}} quanto campos flat na raiz
                Dictionary<string, object> dados;
                if (root.TryGetProperty("dados", out var dadosEl) && dadosEl.ValueKind == JsonValueKind.Object)
                {
                    dados = JsonSerializer.Deserialize<Dictionary<string, object>>(dadosEl.GetRawText()) ?? [];
                }
                else
                {
                    dados = [];
                    foreach (var field in new[] { "especialidade", "data", "hora" })
                        if (root.TryGetProperty(field, out var fieldEl) && fieldEl.ValueKind != JsonValueKind.Null)
                            dados[field] = fieldEl.GetString() ?? "";

                    // Se tem dados de agendamento mas intent veio errado, corrige
                    if (dados.Count > 0 && intent == "duvida")
                        intent = "agendar";
                }

                return new LlmResponse
                {
                    Intent = intent,
                    Resposta = resposta,
                    Dados = dados
                };
            }

            return new LlmResponse { Intent = "duvida", Resposta = "Desculpe, houve um erro ao processar." };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LlmService] Erro: {ex.Message}");
            return new LlmResponse { Intent = "erro", Resposta = "Erro ao processar mensagem." };
        }
    }
}