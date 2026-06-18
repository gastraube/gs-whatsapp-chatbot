namespace gschatbot.api.Models;

public class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EspecialistaId { get; set; }
    public int HorarioConsultaId { get; set; }
    public string Status { get; set; } = "pendente"; // pendente, confirmado, cancelado, realizado
    public string? NotasCliente { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; }
    public Especialista Especialista { get; set; }
    public HorarioConsulta HorarioConsulta { get; set; }
    public ICollection<HistoricoMensagem> HistoricoMensagens { get; set; } = new List<HistoricoMensagem>();
}