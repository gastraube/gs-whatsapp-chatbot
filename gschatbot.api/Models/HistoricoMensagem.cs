namespace gschatbot.api.Models;

public class HistoricoMensagem
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int? AgendamentoId { get; set; }
    public string RemetenteId { get; set; } // "cliente" ou "bot"
    public string Mensagem { get; set; }
    public string Tipo { get; set; } = "texto"; // texto, template, etc
    public string MetadadosJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cliente Cliente { get; set; }
    public Agendamento Agendamento { get; set; }
}
