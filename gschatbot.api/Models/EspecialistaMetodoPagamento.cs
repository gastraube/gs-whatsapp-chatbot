namespace gschatbot.api.Models;

public class EspecialistaMetodoPagamento
{
    public int EspecialistaId { get; set; }
    public int MetodoPagamentoId { get; set; }

    public Especialista Especialista { get; set; } = null!;
    public MetodoPagamento MetodoPagamento { get; set; } = null!;
}
