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

    public async Task<(List<Cliente> Dados, int Total)> ListarPaginadoAsync(
        int pagina, int tamanhoPagina, string? busca = null,
        string? ordenarPor = null, bool crescente = true)
    {
        var query = _context.Clientes
            .Include(c => c.Numeros)
            .Include(c => c.Planos).ThenInclude(p => p.PlanoAssistencia)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(c =>
                (c.Nome != null && c.Nome.Contains(busca)) ||
                (c.Cpf != null && c.Cpf.Contains(busca)) ||
                (c.Email != null && c.Email.Contains(busca)));

        var total = await query.CountAsync();

        IOrderedQueryable<Cliente> ordered = ordenarPor?.ToLower() switch
        {
            "cpf"             => crescente ? query.OrderBy(c => c.Cpf)             : query.OrderByDescending(c => c.Cpf),
            "ativo"           => crescente ? query.OrderBy(c => c.Ativo)           : query.OrderByDescending(c => c.Ativo),
            "datanascimento"  => crescente ? query.OrderBy(c => c.DataNascimento)  : query.OrderByDescending(c => c.DataNascimento),
            _                 => crescente ? query.OrderBy(c => c.Nome)            : query.OrderByDescending(c => c.Nome),
        };

        var dados = await ordered
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (dados, total);
    }

    public async Task<Cliente?> BuscarPorIdAsync(int id) =>
        await _context.Clientes
            .Include(c => c.Numeros)
            .Include(c => c.Planos).ThenInclude(p => p.PlanoAssistencia)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> ExcluirAsync(int id)
    {
        var c = await _context.Clientes.FindAsync(id);
        if (c is null) return false;
        _context.Clientes.Remove(c);
        await _context.SaveChangesAsync();
        return true;
    }
}
