namespace gschatbot.api.Models;

public class PlanoAssistencia
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<EspecialistaPlano> Especialistas { get; set; } = new List<EspecialistaPlano>();
    public ICollection<ClientePlano> Clientes { get; set; } = new List<ClientePlano>();
}
