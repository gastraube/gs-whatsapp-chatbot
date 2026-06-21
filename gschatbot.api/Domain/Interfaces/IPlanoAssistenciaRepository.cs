using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IPlanoAssistenciaRepository
{
    Task<List<string>> ListarNomesAtivosAsync();
    Task<PlanoAssistencia?> BuscarPorNomeAsync(string nome);
    Task<List<string>> ListarPlanosClienteAsync(int clienteId);
    Task<bool> EspecialistaAceitaPlanoAsync(int especialistaId, int planoId);
}
