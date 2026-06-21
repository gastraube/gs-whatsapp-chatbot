using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IMetodoPagamentoRepository
{
    Task<List<string>> ListarNomesPorEspecialistaAsync(int especialistaId);
}
