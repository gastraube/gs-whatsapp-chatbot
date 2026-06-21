using gschatbot.api.Data;
using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EspecialistasController : ControllerBase
{
    private readonly IEspecialistaRepository _repo;
    private readonly AppDbContext _context;

    public EspecialistasController(IEspecialistaRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
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
            Dados = dados.Select(e => (object)new
            {
                id = e.Id,
                nome = e.Nome,
                crm = e.Crm,
                especialidade = e.Especialidade?.Nome ?? "",
                telefone = e.Telefone ?? "",
                email = e.Email ?? "",
                ativo = e.Ativo,
            }).ToList(),
            Total = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
        };

        return Ok(resposta);
    }

    [HttpGet("especialidades")]
    public async Task<IActionResult> ListarEspecialidades()
    {
        var lista = await _context.Especialidades
            .OrderBy(e => e.Nome)
            .Select(e => new { e.Id, e.Nome })
            .ToListAsync();
        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var e = await _repo.BuscarPorIdAsync(id);
        if (e is null) return NotFound();
        return Ok(new
        {
            id = e.Id,
            nome = e.Nome,
            crm = e.Crm,
            especialidadeId = e.EspecialidadeId,
            especialidade = e.Especialidade?.Nome ?? "",
            telefone = e.Telefone ?? "",
            email = e.Email ?? "",
            ativo = e.Ativo,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] EspecialistaRequest req)
    {
        var especialista = new Especialista
        {
            Nome = req.Nome,
            Crm = req.Crm,
            EspecialidadeId = req.EspecialidadeId,
            Telefone = req.Telefone ?? "",
            Email = req.Email ?? "",
            Ativo = req.Ativo,
        };
        await _repo.CriarAsync(especialista);
        return CreatedAtAction(nameof(BuscarPorId), new { id = especialista.Id }, new { especialista.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EspecialistaRequest req)
    {
        var e = await _repo.BuscarPorIdAsync(id);
        if (e is null) return NotFound();

        e.Nome = req.Nome;
        e.Crm = req.Crm;
        e.EspecialidadeId = req.EspecialidadeId;
        e.Telefone = req.Telefone ?? "";
        e.Email = req.Email ?? "";
        e.Ativo = req.Ativo;

        await _repo.AtualizarAsync(e);
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

public record EspecialistaRequest(
    string Nome,
    string Crm,
    int EspecialidadeId,
    string? Telefone,
    string? Email,
    bool Ativo
);
