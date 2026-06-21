using gschatbot.api.Data;
using gschatbot.api.Domain;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class HorarioConsultaRepository : IHorarioConsultaRepository
{
    private readonly AppDbContext _context;

    public HorarioConsultaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SlotDisponivel>> ListarPorEspecialidadeAsync(int especialidadeId, int quantidade)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        return await (
            from h in _context.HorariosConsulta
            join e in _context.Especialistas on h.EspecialistaId equals e.Id
            where e.EspecialidadeId == especialidadeId && e.Ativo
                  && h.Status == "disponivel" && h.DataConsulta >= hoje
            orderby h.DataConsulta, h.HoraInicio
            select new SlotDisponivel(h.Id, e.Id, e.Nome, h.DataConsulta, h.HoraInicio)
        ).Take(quantidade).ToListAsync();
    }

    public async Task<List<SlotDisponivel>> ListarPorEspecialistaAsync(int especialistaId, int quantidade)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        return await (
            from h in _context.HorariosConsulta
            join e in _context.Especialistas on h.EspecialistaId equals e.Id
            where h.EspecialistaId == especialistaId && h.Status == "disponivel" && h.DataConsulta >= hoje
            orderby h.DataConsulta, h.HoraInicio
            select new SlotDisponivel(h.Id, e.Id, e.Nome, h.DataConsulta, h.HoraInicio)
        ).Take(quantidade).ToListAsync();
    }

    public async Task<HorarioConsulta?> BuscarDisponivelComDetalhesAsync(int horarioId)
    {
        return await _context.HorariosConsulta
            .Include(h => h.Especialista).ThenInclude(e => e.Especialidade)
            .Include(h => h.Endereco)
            .FirstOrDefaultAsync(h => h.Id == horarioId && h.Status == "disponivel");
    }

    public async Task<HorarioConsulta?> BuscarPorDetalhesAsync(int especialistaId, DateOnly data, TimeOnly hora)
    {
        return await _context.HorariosConsulta
            .Include(h => h.Especialista).ThenInclude(e => e.Especialidade)
            .Include(h => h.Endereco)
            .FirstOrDefaultAsync(h =>
                h.EspecialistaId == especialistaId &&
                h.DataConsulta == data &&
                h.HoraInicio == hora &&
                h.Status == "disponivel");
    }

    public async Task ReservarAsync(HorarioConsulta horario)
    {
        horario.Status = "reservado";
        await _context.SaveChangesAsync();
    }
}
