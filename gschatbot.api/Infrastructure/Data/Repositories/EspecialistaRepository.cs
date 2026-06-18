using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class EspecialistaRepository : IEspecialistaRepository
{
    private readonly AppDbContext _context;

    public EspecialistaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Especialista?> BuscarPorNomeAsync(string nome)
    {
        return await _context.Especialistas
            .FirstOrDefaultAsync(e => e.Nome.ToLower().Contains(nome.ToLower()) && e.Ativo);
    }

    public async Task<List<string>> ListarNomesAtivosAsync()
    {
        return await _context.Especialistas
            .Where(e => e.Ativo)
            .Select(e => e.Nome)
            .ToListAsync();
    }

    public async Task<List<Especialista>> ListarPorEspecialidadeAsync(int especialidadeId)
    {
        return await _context.Especialistas
            .Where(e => e.EspecialidadeId == especialidadeId && e.Ativo)
            .ToListAsync();
    }
}
