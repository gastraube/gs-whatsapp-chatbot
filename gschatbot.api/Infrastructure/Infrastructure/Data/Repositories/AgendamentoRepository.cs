using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;

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
}
