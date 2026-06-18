namespace gschatbot.api.Models;

public class Especialista
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Crm { get; set; }
    public int EspecialidadeId { get; set; }
    public string Telefone { get; set; }
    public string Email { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Especialidade Especialidade { get; set; }
    public ICollection<EspecialistaEndereco> EspecialistasEnderecos { get; set; } = new List<EspecialistaEndereco>();
public ICollection<HorarioConsulta> HorariosConsulta { get; set; } = new List<HorarioConsulta>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
