namespace gschatbot.api.Models;

public class ClientePlano
{
    public int ClienteId { get; set; }
    public int PlanoAssistenciaId { get; set; }
    public string? NumeroCarteirinha { get; set; }

    public Cliente Cliente { get; set; }
    public PlanoAssistencia PlanoAssistencia { get; set; }
}
