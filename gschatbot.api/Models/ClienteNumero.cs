namespace gschatbot.api.Models;

public class ClienteNumero
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Numero { get; set; }
    public bool Principal { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; }
}
