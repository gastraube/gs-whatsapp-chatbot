using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class MetodoPagamentoRepository : IMetodoPagamentoRepository
{
    private readonly AppDbContext _context;

    public MetodoPagamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> ListarNomesPorEspecialistaAsync(int especialistaId)
    {
        return await _context.EspecialistaMetodosPagamento
            .Where(emp => emp.EspecialistaId == especialistaId)
            .Select(emp => emp.MetodoPagamento.Nome)
            .ToListAsync();
    }
}
