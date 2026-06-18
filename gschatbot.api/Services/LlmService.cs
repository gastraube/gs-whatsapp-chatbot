using gschatbot.api.Models;
using System.Text.Json;

namespace gschatbot.api.Services;

// Responsável por toda comunicação com o modelo de linguagem local (Ollama).
// Fluxo: monta o prompt → chama a API do Ollama → faz parse do JSON retornado → devolve LlmResponse.
public class LlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public LlmService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    // Ponto de entrada principal. Recebe a mensagem do cliente e contextos opcionais
    // (histórico da conversa, listas de especialidades e médicos para o LLM normalizar nomes)
    // e devolve a intenção detectada + dados estruturados.
    public async Task<LlmResponse> ProcessMessage(
        string userMessage,
        List<(string role, string message)>? historico = null,
        List<string>? especialidades = null,
        List<string>? especialistas = null)
    {
        try
        {
            var prompt = BuildPrompt(userMessage, historico, especialidades, especialistas);
            var json = await CallOllama(prompt);
            return ParseResponse(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LlmService] Erro: {ex.Message}");
            return new LlmResponse { Intent = "erro", Resposta = "Erro ao processar mensagem." };
        }
    }

    // Monta o prompt enviado ao modelo.
    // Injeta dinamicamente: histórico da conversa, especialidades e médicos cadastrados.
    // O modelo deve responder SEMPRE em JSON com os campos: intent, resposta e dados.
    // Não pedimos data/hora aqui — os horários disponíveis são buscados no banco pelo AgendamentoHandler.
    private static string BuildPrompt(
        string userMessage,
        List<(string role, string message)>? historico,
        List<string>? especialidades,
        List<string>? especialistas)
    {
        // Bloco de histórico — incluído só quando existe, para economizar tokens
        var historicoBloco = historico?.Count > 0
            ? "\n\n--- Histórico recente ---\n" +
              string.Join("\n", historico.Select(h => $"{h.role}: {h.message}")) +
              "\n--- Fim ---\n"
            : "";

        // Listas enviadas para que o modelo use exatamente os nomes cadastrados no banco,
        // evitando variações como "Cardio" em vez de "Cardiologia".
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

        Mensagem do cliente: {userMessage}";
    }

    // Chama a API de geração de texto do Ollama e retorna o conteúdo bruto da resposta.
    // O Ollama envelopa a resposta do modelo dentro do campo "response" do JSON retornado.
    private async Task<string> CallOllama(string prompt)
    {
        var ollamaUrl = _config["Ollama:Url"] ?? "http://localhost:11434";
        var model = _config["Ollama:Model"] ?? "mistral";

        var body = JsonSerializer.Serialize(new { model, prompt, stream = false, format = "json" });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ollamaUrl}/api/generate", content);
        var text = await response.Content.ReadAsStringAsync();

        // Extrai apenas o texto gerado pelo modelo, descartando os metadados do Ollama
        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.GetProperty("response").GetString() ?? "";
    }

    // Faz parse do JSON gerado pelo modelo e retorna um LlmResponse tipado.
    // O modelo às vezes adiciona texto antes/depois do JSON — por isso buscamos
    // o primeiro '{' e o último '}' para extrair apenas o objeto JSON.
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
            // Mistral às vezes gera escapes duplos (\" em vez de ") — limpamos antes de tentar novamente
            var cleaned = jsonStr.Replace("\\\"", "\"").Replace("\\r\\n", "").Replace("\\n", "").Replace("\\t", " ");
            root = JsonDocument.Parse(cleaned).RootElement;
        }

        var intent = root.TryGetProperty("intent", out var i) ? i.GetString() ?? "duvida" : "duvida";
        var resposta = root.TryGetProperty("resposta", out var r) ? r.GetString() ?? "Desculpe, não entendi." : "Desculpe, não entendi.";

        // "dados" contém tipo + especialidade/medico quando intent = "agendar"
        var dados = root.TryGetProperty("dados", out var d) && d.ValueKind == JsonValueKind.Object
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(d.GetRawText()) ?? []
            : [];

        return new LlmResponse { Intent = intent, Resposta = resposta, Dados = dados };
    }
}
