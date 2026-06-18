using gschatbot.api.Data;
using gschatbot.api.Models;
using gschatbot.api.Services;
using gschatbot.api.Services.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly LlmService _llmService;
    private readonly TwilioService _twilioService;
    private readonly IntentDispatcher _intentDispatcher;
    private readonly AgendamentoHandler _agendamentoHandler;
    private readonly AppDbContext _context;

    public WhatsAppController(
        LlmService llmService,
        TwilioService twilioService,
        IntentDispatcher intentDispatcher,
        AgendamentoHandler agendamentoHandler,
        AppDbContext context)
    {
        _llmService = llmService;
        _twilioService = twilioService;
        _intentDispatcher = intentDispatcher;
        _agendamentoHandler = agendamentoHandler;
        _context = context;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromForm] IFormCollection form)
    {
        try
        {
            var fromNumber = form["From"].ToString();
            var messageBody = form["Body"].ToString();

            Console.WriteLine($"[WhatsAppController] Recebido de {fromNumber}: {messageBody}");

            var numeroLimpo = fromNumber.Replace("whatsapp:", "").Replace("+", "").Replace(" ", "");
            var cliente = await _twilioService.GetOrCreateCliente(numeroLimpo);

            // Verifica se está aguardando uma escolha da lista (bypass do LLM)
            var sessao = await _context.SessoesConversa
                .FirstOrDefaultAsync(s => s.ClienteId == cliente.Id);

            if (sessao?.EstadoAtual == "aguardando_escolha")
            {
                await _twilioService.SaveHistoricoMensagem(cliente.Id, null, "cliente", messageBody);
                await _agendamentoHandler.ProcessarEscolha(cliente.Id, numeroLimpo, messageBody, sessao);
                return Ok();
            }

            // Fluxo normal via LLM
            var historico = await _context.HistoricoMensagens
                .Where(h => h.ClienteId == cliente.Id)
                .OrderByDescending(h => h.CreatedAt)
                .Take(10)
                .OrderBy(h => h.CreatedAt)
                .Select(h => new { h.RemetenteId, h.Mensagem })
                .ToListAsync();

            var historicoTuples = historico
                .Select(h => (h.RemetenteId == "cliente" ? "Cliente" : "Bot", h.Mensagem))
                .ToList();

            var especialidades = await _context.Especialidades
                .Select(e => e.Nome)
                .ToListAsync();

            var especialistas = await _context.Especialistas
                .Where(e => e.Ativo)
                .Select(e => e.Nome)
                .ToListAsync();

            var llmResponse = await _llmService.ProcessMessage(messageBody, historicoTuples, especialidades, especialistas);

            await _twilioService.SaveHistoricoMensagem(cliente.Id, null, "cliente", messageBody);

            if (sessao != null)
            {
                sessao.EstadoAtual = llmResponse.Intent;
                sessao.UltimaMensagemEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var handled = await _intentDispatcher.Dispatch(cliente.Id, numeroLimpo, llmResponse);

            if (!handled)
            {
                await _twilioService.SendMessage(numeroLimpo, llmResponse.Resposta);
                await _twilioService.SaveHistoricoMensagem(cliente.Id, null, "bot", llmResponse.Resposta);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WhatsAppController] Erro: {ex.Message}");
            return Ok();
        }
    }

    [HttpGet("webhook")]
    public IActionResult WebhookTest() => Ok(new { message = "Webhook de WhatsApp está funcionando!" });

    [HttpGet("clientes")]
    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _context.Clientes
            .Include(c => c.Agendamentos)
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("clientes/{id}/historico")]
    public async Task<IActionResult> HistoricoCliente(int id)
    {
        var historico = await _context.HistoricoMensagens
            .Where(h => h.ClienteId == id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();

        return Ok(historico);
    }
}
