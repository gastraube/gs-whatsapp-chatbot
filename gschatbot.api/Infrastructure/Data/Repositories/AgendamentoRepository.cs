using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AppDbContext _context;

    public AgendamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Agendamento> CriarAsync(Agendamento agendamento)
    {
        _context.Agendamentos.Add(agendamento);
        await _context.SaveChangesAsync();
        return agendamento;
    }

    public async Task<List<Agendamento>> ListarFuturosAsync(int clienteId)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        return await _context.Agendamentos
            .Include(a => a.Especialista).ThenInclude(e => e.Especialidade)
            .Include(a => a.HorarioConsulta)
            .Where(a => a.ClienteId == clienteId
                && a.Status != StatusAgendamento.Cancelado
                && a.HorarioConsulta.DataConsulta >= hoje)
            .OrderBy(a => a.HorarioConsulta.DataConsulta)
            .ThenBy(a => a.HorarioConsulta.HoraInicio)
            .ToListAsync();
    }

    public async Task CancelarAsync(int agendamentoId)
    {
        var agendamento = await _context.Agendamentos
            .Include(a => a.HorarioConsulta)
            .FirstOrDefaultAsync(a => a.Id == agendamentoId);

        if (agendamento == null) return;

        agendamento.Status = StatusAgendamento.Cancelado;
        agendamento.HorarioConsulta.Status = "disponivel";
        await _context.SaveChangesAsync();
    }
}
