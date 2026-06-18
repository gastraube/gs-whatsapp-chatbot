namespace gschatbot.api.Domain;

public record SlotDisponivel(
    int SlotId,
    int EspecialistaId,
    string EspecialistaNome,
    DateOnly DataConsulta,
    TimeOnly HoraInicio);
