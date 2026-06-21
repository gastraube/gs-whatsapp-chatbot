using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace gschatbot.api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteRepository _repo;

    public ClientesController(IClienteRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        [FromQuery] string? busca = null,
        [FromQuery] string? ordenarPor = "nome",
        [FromQuery] bool crescente = true)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 1, 100);

        var (dados, total) = await _repo.ListarPaginadoAsync(pagina, tamanhoPagina, busca, ordenarPor, crescente);

        var resposta = new PaginadoResponse<object>
        {
            Dados = dados.Select(c => (object)new
            {
                id = c.Id,
                nome = c.Nome,
                cpf = c.Cpf,
                email = c.Email,
                dataNascimento = c.DataNascimento.HasValue
                    ? c.DataNascimento.Value.ToString("yyyy-MM-dd")
                    : null,
                numeroPrincipal = c.Numeros.FirstOrDefault(n => n.Principal)?.Numero
                    ?? c.Numeros.FirstOrDefault()?.Numero,
                planoPrincipal = c.Planos.FirstOrDefault()?.PlanoAssistencia?.Nome,
                ativo = c.Ativo,
            }).ToList(),
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
        };

        return Ok(resposta);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var c = await _repo.BuscarPorIdAsync(id);
        if (c is null) return NotFound();
        return Ok(new
        {
            id = c.Id,
            nome = c.Nome,
            cpf = c.Cpf,
            email = c.Email,
            dataNascimento = c.DataNascimento.HasValue
                ? c.DataNascimento.Value.ToString("yyyy-MM-dd")
                : null,
            ativo = c.Ativo,
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteRequest req)
    {
        var c = await _repo.BuscarPorIdAsync(id);
        if (c is null) return NotFound();

        c.Nome = req.Nome;
        c.Cpf = req.Cpf;
        c.Email = req.Email;
        c.DataNascimento = req.DataNascimento.HasValue
            ? DateTime.SpecifyKind(req.DataNascimento.Value, DateTimeKind.Utc)
            : null;
        c.Ativo = req.Ativo;

        await _repo.AtualizarDadosAsync(c);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var excluido = await _repo.ExcluirAsync(id);
        if (!excluido) return NotFound();
        return NoContent();
    }
}

public record ClienteRequest(
    string? Nome,
    string? Cpf,
    string? Email,
    DateTime? DataNascimento,
    bool Ativo
);
