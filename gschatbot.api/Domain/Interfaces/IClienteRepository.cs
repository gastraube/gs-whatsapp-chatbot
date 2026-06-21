using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente> ObterOuCriarAsync(string numeroWhatsApp);
    Task<List<Cliente>> ListarComAgendamentosAsync();
    Task AtualizarDadosAsync(Cliente cliente);
    Task<(List<Cliente> Dados, int Total)> ListarPaginadoAsync(
        int pagina, int tamanhoPagina, string? busca = null,
        string? ordenarPor = null, bool crescente = true);
    Task<Cliente?> BuscarPorIdAsync(int id);
    Task<bool> ExcluirAsync(int id);
}
