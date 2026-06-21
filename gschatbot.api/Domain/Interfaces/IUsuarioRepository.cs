using gschatbot.api.Models;

namespace gschatbot.api.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<Usuario?> BuscarPorIdAsync(int id);
    Task<List<Usuario>> ListarAsync();
    Task CriarAsync(Usuario usuario);
}
