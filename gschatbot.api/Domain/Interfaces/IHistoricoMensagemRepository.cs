using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IHistoricoMensagemRepository
{
    Task<List<HistoricoMensagem>> ListarRecentesAsync(int clienteId, int quantidade);
    Task<List<HistoricoMensagem>> ListarPorClienteAsync(int clienteId);
    Task AdicionarAsync(HistoricoMensagem mensagem);
}
