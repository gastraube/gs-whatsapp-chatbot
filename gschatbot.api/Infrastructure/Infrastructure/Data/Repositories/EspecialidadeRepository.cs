using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class EspecialidadeRepository : IEspecialidadeRepository
{
    private readonly AppDbContext _context;

    public EspecialidadeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Especialidade?> BuscarPorNomeAsync(string nome)
    {
        return await _context.Especialidades
            .FirstOrDefaultAsync(e => e.Nome.ToLower().Contains(nome.ToLower()));
    }

    public async Task<List<string>> ListarNomesAsync()
    {
        return await _context.Especialidades
            .Select(e => e.Nome)
            .ToListAsync();
    }
}
