using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class SessaoConversaRepository : ISessaoConversaRepository
{
    private readonly AppDbContext _context;

    public SessaoConversaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SessaoConversa?> BuscarPorClienteAsync(int clienteId)
    {
        return await _context.SessoesConversa
            .FirstOrDefaultAsync(s => s.ClienteId == clienteId);
    }

    public async Task<SessaoConversa> CriarAsync(SessaoConversa sessao)
    {
        _context.SessoesConversa.Add(sessao);
        await _context.SaveChangesAsync();
        return sessao;
    }

    public async Task AtualizarAsync(SessaoConversa sessao)
    {
        await _context.SaveChangesAsync();
    }
}
