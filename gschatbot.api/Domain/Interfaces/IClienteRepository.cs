using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IClienteRepository
{
    Task<Cliente> ObterOuCriarAsync(string numeroWhatsApp);
    Task<List<Cliente>> ListarComAgendamentosAsync();
    Task AtualizarDadosAsync(Cliente cliente);
}
