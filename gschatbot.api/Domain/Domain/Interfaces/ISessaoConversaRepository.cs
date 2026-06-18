using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface ISessaoConversaRepository
{
    Task<SessaoConversa?> BuscarPorClienteAsync(int clienteId);
    Task<SessaoConversa> CriarAsync(SessaoConversa sessao);
    Task AtualizarAsync(SessaoConversa sessao);
}
