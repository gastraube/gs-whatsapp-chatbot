using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface ILlmService
{
    Task<LlmResponse> ProcessarMensagemAsync(
        string mensagem,
        List<(string role, string texto)>? historico = null,
        List<string>? especialidades = null,
        List<string>? especialistas = null);
}
