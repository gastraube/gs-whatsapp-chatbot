namespace gschatbot.api.Models;

public class Cliente
{
    public int Id { get; set; }
    public string NumeroWhatsApp { get; set; }
    public string Nome { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relacionamentos
    public SessaoConversa? Sessao { get; set; }
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<HistoricoMensagem> HistoricoMensagens { get; set; } = new List<HistoricoMensagem>();
}