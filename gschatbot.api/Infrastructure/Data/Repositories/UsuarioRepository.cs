using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Infrastructure.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

    public async Task<Usuario?> BuscarPorIdAsync(int id) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Ativo);

    public async Task<List<Usuario>> ListarAsync() =>
        await _context.Usuarios.Where(u => u.Ativo).ToListAsync();

    public async Task CriarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
    }
}
