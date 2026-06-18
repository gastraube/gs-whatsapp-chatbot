namespace gschatbot.api.Models;

public class EspecialistaEndereco
{
    public int Id { get; set; }
    public int EspecialistaId { get; set; }
    public int EnderecoId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Especialista Especialista { get; set; }
    public Endereco Endereco { get; set; }
}
