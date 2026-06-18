namespace gschatbot.api.Application.Agendamento;

public class ContextoEscolha
{
    public List<SlotContexto> Slots { get; set; } = [];
    public string TipoBusca { get; set; } = "";
    public string NomeBusca { get; set; } = "";
    public int Offset { get; set; }
}
