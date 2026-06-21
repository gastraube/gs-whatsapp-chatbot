using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IEspecialistaRepository
{
    Task<Especialista?> BuscarPorNomeAsync(string nome);
    Task<List<string>> ListarNomesAtivosAsync();
    Task<List<Especialista>> ListarAtivosAsync();
    Task<List<Especialista>> ListarPorEspecialidadeAsync(int especialidadeId);
}
