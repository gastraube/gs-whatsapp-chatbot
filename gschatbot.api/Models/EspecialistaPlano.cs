namespace gschatbot.api.Models;

public class EspecialistaPlano
{
    public int EspecialistaId { get; set; }
    public int PlanoAssistenciaId { get; set; }

    public Especialista Especialista { get; set; }
    public PlanoAssistencia PlanoAssistencia { get; set; }
}
