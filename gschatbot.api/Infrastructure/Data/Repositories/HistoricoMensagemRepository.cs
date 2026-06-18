using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class HistoricoMensagemRepository : IHistoricoMensagemRepository
{
    private readonly AppDbContext _context;

    public HistoricoMensagemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HistoricoMensagem>> ListarRecentesAsync(int clienteId, int quantidade)
    {
        return await _context.HistoricoMensagens
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.CreatedAt)
            .Take(quantidade)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<HistoricoMensagem>> ListarPorClienteAsync(int clienteId)
    {
        return await _context.HistoricoMensagens
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task AdicionarAsync(HistoricoMensagem mensagem)
    {
        _context.HistoricoMensagens.Add(mensagem);
        await _context.SaveChangesAsync();
    }
}
