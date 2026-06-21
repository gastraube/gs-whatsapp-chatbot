namespace gschatbot.api.Models;

public class PaginadoResponse<T>
{
    public List<T> Dados { get; set; } = new();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas => TamanhoPagina > 0
        ? (int)Math.Ceiling((double)Total / TamanhoPagina)
        : 0;
}
