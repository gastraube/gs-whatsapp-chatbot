namespace gschatbot.api.Models;

public class MetodoPagamento
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<EspecialistaMetodoPagamento> Especialistas { get; set; } = new List<EspecialistaMetodoPagamento>();
}
