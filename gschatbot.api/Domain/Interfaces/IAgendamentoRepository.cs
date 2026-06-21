using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IAgendamentoRepository
{
    Task<Agendamento> CriarAsync(Agendamento agendamento);
    Task<List<Agendamento>> ListarFuturosAsync(int clienteId);
    Task CancelarAsync(int agendamentoId);
}
