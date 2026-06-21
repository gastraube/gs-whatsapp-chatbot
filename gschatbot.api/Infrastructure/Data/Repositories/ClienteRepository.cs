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
        var clienteExistente = await _context.ClienteNumeros
            .Where(cn => cn.Numero == numeroWhatsApp)
            .Select(cn => cn.Cliente)
            .FirstOrDefaultAsync();

        if (clienteExistente != null)
            return clienteExistente;

        var cliente = new Cliente { Ativo = true };
        cliente.Numeros.Add(new ClienteNumero { Numero = numeroWhatsApp, Principal = true });

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return cliente;
    }

    public async Task<List<Cliente>> ListarComAgendamentosAsync()
    {
        return await _context.Clientes
            .Include(c => c.Numeros)
            .Include(c => c.Agendamentos)
            .ToListAsync();
    }

    public async Task AtualizarDadosAsync(Cliente cliente)
    {
        cliente.UpdatedAt = DateTime.UtcNow;
        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
    }
}
