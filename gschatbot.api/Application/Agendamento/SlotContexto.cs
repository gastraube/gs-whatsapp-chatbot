namespace gschatbot.api.Application.Agendamento;

public class SlotContexto
{
    public int Index { get; set; }
    public int SlotId { get; set; }
    public string EspecialistaNome { get; set; } = "";
    public string Data { get; set; } = "";
    public string Hora { get; set; } = "";
}
