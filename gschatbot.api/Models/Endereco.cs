namespace gschatbot.api.Models;

public class Endereco
{
    public int Id { get; set; }
    public string Rua { get; set; }
    public string Numero { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Cidade { get; set; }
    public string Estado { get; set; }
    public string CEP { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HorarioConsulta> HorariosConsulta { get; set; } = new List<HorarioConsulta>();
}
