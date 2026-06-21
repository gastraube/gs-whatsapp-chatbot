using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class PlanoAssistenciaRepository : IPlanoAssistenciaRepository
{
    private readonly AppDbContext _context;

    public PlanoAssistenciaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> ListarNomesAtivosAsync()
    {
        return await _context.PlanosAssistencia
            .Where(p => p.Ativo)
            .Select(p => p.Nome)
            .ToListAsync();
    }

    public async Task<PlanoAssistencia?> BuscarPorNomeAsync(string nome)
    {
        return await _context.PlanosAssistencia
            .FirstOrDefaultAsync(p => p.Nome.ToLower().Contains(nome.ToLower()) && p.Ativo);
    }

    public async Task<List<string>> ListarPlanosClienteAsync(int clienteId)
    {
        return await _context.ClientePlanos
            .Where(cp => cp.ClienteId == clienteId)
            .Select(cp => cp.PlanoAssistencia.Nome)
            .ToListAsync();
    }

    public async Task<bool> EspecialistaAceitaPlanoAsync(int especialistaId, int planoId)
    {
        return await _context.EspecialistaPlanos
            .AnyAsync(ep => ep.EspecialistaId == especialistaId && ep.PlanoAssistenciaId == planoId);
    }
}
