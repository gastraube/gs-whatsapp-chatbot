namespace gschatbot.api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string? Cpf { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public DateTime? DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relacionamentos
    public ICollection<ClienteNumero> Numeros { get; set; } = new List<ClienteNumero>();
    public ICollection<ClientePlano> Planos { get; set; } = new List<ClientePlano>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<HistoricoMensagem> HistoricoMensagens { get; set; } = new List<HistoricoMensagem>();
}