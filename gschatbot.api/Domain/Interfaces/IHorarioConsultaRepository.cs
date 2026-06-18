using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IHorarioConsultaRepository
{
    Task<List<SlotDisponivel>> ListarPorEspecialidadeAsync(int especialidadeId, int offset, int quantidade);
    Task<List<SlotDisponivel>> ListarPorEspecialistaAsync(int especialistaId, int offset, int quantidade);
    Task<HorarioConsulta?> BuscarDisponivelComDetalhesAsync(int horarioId);
    Task ReservarAsync(HorarioConsulta horario);
}
