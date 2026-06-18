namespace gschatbot.api.Models;

public class SessaoConversa
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string EstadoAtual { get; set; } = "inicial"; // inicial, esperando_especialidade, esperando_data, confirmando, etc
    public string ContextoJson { get; set; } = "{}";
    public DateTime UltimaMensagemEm { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; }
}
