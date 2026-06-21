namespace gschatbot.api.Models;

public class Agendamento
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int EspecialistaId { get; set; }
    public int HorarioConsultaId { get; set; }
    public StatusAgendamento Status { get; set; } = StatusAgendamento.Pendente;
    public string TipoPagamento { get; set; } = "particular"; // particular | plano
    public int? PlanoAssistenciaId { get; set; }
    public string? NotasCliente { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; }
    public Especialista Especialista { get; set; }
    public HorarioConsulta HorarioConsulta { get; set; }
    public PlanoAssistencia? PlanoAssistencia { get; set; }
    public ICollection<HistoricoMensagem> HistoricoMensagens { get; set; } = new List<HistoricoMensagem>();
}