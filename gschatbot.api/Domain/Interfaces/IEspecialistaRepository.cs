using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IEspecialistaRepository
{
    Task<Especialista?> BuscarPorNomeAsync(string nome);
    Task<List<string>> ListarNomesAtivosAsync();
    Task<List<Especialista>> ListarAtivosAsync();
    Task<List<Especialista>> ListarPorEspecialidadeAsync(int especialidadeId);
    Task<(List<Especialista> Dados, int Total)> ListarPaginadoAsync(
        int pagina, int tamanhoPagina, string? busca = null,
        string? ordenarPor = null, bool crescente = true);
    Task<Especialista?> BuscarPorIdAsync(int id);
    Task<Especialista> CriarAsync(Especialista especialista);
    Task AtualizarAsync(Especialista especialista);
    Task<bool> ExcluirAsync(int id);
}
