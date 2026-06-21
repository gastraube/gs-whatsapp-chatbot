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

    public async Task<List<Especialista>> ListarAtivosAsync()
    {
        return await _context.Especialistas
            .Where(e => e.Ativo)
            .ToListAsync();
    }

    public async Task<List<Especialista>> ListarPorEspecialidadeAsync(int especialidadeId)
    {
        return await _context.Especialistas
            .Where(e => e.EspecialidadeId == especialidadeId && e.Ativo)
            .ToListAsync();
    }

    public async Task<(List<Especialista> Dados, int Total)> ListarPaginadoAsync(
        int pagina, int tamanhoPagina, string? busca = null,
        string? ordenarPor = null, bool crescente = true)
    {
        var query = _context.Especialistas
            .Include(e => e.Especialidade)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(e =>
                e.Nome.Contains(busca) ||
                e.Crm.Contains(busca) ||
                e.Email.Contains(busca));

        var total = await query.CountAsync();

        IOrderedQueryable<Especialista> ordered = ordenarPor?.ToLower() switch
        {
            "crm"          => crescente ? query.OrderBy(e => e.Crm)                : query.OrderByDescending(e => e.Crm),
            "especialidade"=> crescente ? query.OrderBy(e => e.Especialidade.Nome) : query.OrderByDescending(e => e.Especialidade.Nome),
            "ativo"        => crescente ? query.OrderBy(e => e.Ativo)              : query.OrderByDescending(e => e.Ativo),
            _              => crescente ? query.OrderBy(e => e.Nome)               : query.OrderByDescending(e => e.Nome),
        };

        var dados = await ordered
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (dados, total);
    }

    public async Task<Especialista?> BuscarPorIdAsync(int id) =>
        await _context.Especialistas
            .Include(e => e.Especialidade)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Especialista> CriarAsync(Especialista especialista)
    {
        _context.Especialistas.Add(especialista);
        await _context.SaveChangesAsync();
        return especialista;
    }

    public async Task AtualizarAsync(Especialista especialista)
    {
        _context.Especialistas.Update(especialista);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var e = await _context.Especialistas.FindAsync(id);
        if (e is null) return false;
        _context.Especialistas.Remove(e);
        await _context.SaveChangesAsync();
        return true;
    }
}
