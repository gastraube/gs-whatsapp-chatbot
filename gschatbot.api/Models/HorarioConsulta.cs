namespace gschatbot.api.Models;

public class HorarioConsulta
{
    public int Id { get; set; }
    public int EspecialistaId { get; set; }
    public int EnderecoId { get; set; }
    public DateOnly DataConsulta { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    public string Status { get; set; } = "disponivel"; // disponivel, reservado, cancelado
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Especialista Especialista { get; set; }
    public Endereco Endereco { get; set; }
    public Agendamento Agendamento { get; set; }
}
