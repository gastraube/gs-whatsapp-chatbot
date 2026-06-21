using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IHorarioConsultaRepository
{
    Task<List<SlotDisponivel>> ListarPorEspecialidadeAsync(int especialidadeId, int quantidade);
    Task<List<SlotDisponivel>> ListarPorEspecialistaAsync(int especialistaId, int quantidade);
    Task<HorarioConsulta?> BuscarDisponivelComDetalhesAsync(int horarioId);
    Task<HorarioConsulta?> BuscarPorDetalhesAsync(int especialistaId, DateOnly data, TimeOnly hora);
    Task ReservarAsync(HorarioConsulta horario);
}
