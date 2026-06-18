using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente> ObterOuCriarAsync(string numeroWhatsApp)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.NumeroWhatsApp == numeroWhatsApp);

        if (cliente != null)
            return cliente;

        cliente = new Cliente
        {
            NumeroWhatsApp = numeroWhatsApp,
            Nome = "Cliente",
            Ativo = true
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var sessao = new SessaoConversa
        {
            ClienteId = cliente.Id,
            EstadoAtual = "inicial"
        };

        _context.SessoesConversa.Add(sessao);
        await _context.SaveChangesAsync();

        return cliente;
    }

    public async Task<List<Cliente>> ListarComAgendamentosAsync()
    {
        return await _context.Clientes
            .Include(c => c.Agendamentos)
            .ToListAsync();
    }
}
