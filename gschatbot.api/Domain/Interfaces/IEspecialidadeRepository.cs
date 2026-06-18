using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IEspecialidadeRepository
{
    Task<Especialidade?> BuscarPorNomeAsync(string nome);
    Task<List<string>> ListarNomesAsync();
}
