namespace gschatbot.api.Models;

public class Especialidade
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Especialista> Especialistas { get; set; } = new List<Especialista>();
}
