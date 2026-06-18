using gschatbot.api.Data;
using gschatbot.api.Models;
using gschatbot.api.Services;
using gschatbot.api.Services.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace gschatbot.api.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly LlmService _llmService;
    private readonly TwilioService _twilioService;
    private readonly IntentDispatcher _intentDispatcher;
    private readonly AppDbContext _context;

    public WhatsAppController(
        LlmService llmService,
        TwilioService twilioService,
        IntentDispatcher intentDispatcher,
        AppDbContext context)
    {
        _llmService = llmService;
        _twilioService = twilioService;
        _intentDispatcher = intentDispatcher;
        _context = context;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromForm] IFormCollection form)
    {
        try
        {
            var fromNumber = form["From"].ToString(); // +55 15 99667-0943
            var messageBody = form["Body"].ToString();
            var messageSid = form["MessageSid"].ToString();

            Console.WriteLine($"[WhatsAppController] Recebido de {fromNumber}: {messageBody}");

            // Normaliza número
            var numeroLimpo = fromNumber.Replace("whatsapp:", "").Replace("+", "").Replace(" ", "");

            // Busca ou cria cliente
            var cliente = await _twilioService.GetOrCreateCliente(numeroLimpo);

            // Busca histórico recente para dar contexto ao LLM
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

            // Especialidades disponíveis para o LLM normalizar o nome
            var especialidades = await _context.Especialidades
                .Select(e => e.Nome)
                .ToListAsync();

            // Processa com LLM
            var llmResponse = await _llmService.ProcessMessage(messageBody, historicoTuples, especialidades);

            // Salva histórico da mensagem do cliente
            await _twilioService.SaveHistoricoMensagem(cliente.Id, null, "cliente", messageBody);

            // Atualiza sessão
            var sessao = await _context.SessoesConversa
                .FirstOrDefaultAsync(s => s.ClienteId == cliente.Id);

            if (sessao != null)
            {
                sessao.EstadoAtual = llmResponse.Intent;
                sessao.UltimaMensagemEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            // Dispatch para handler
            await _intentDispatcher.Dispatch(cliente.Id, numeroLimpo, llmResponse);

            // Salva resposta do bot no histórico
            await _twilioService.SaveHistoricoMensagem(cliente.Id, null, "bot", llmResponse.Resposta);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WhatsAppController] Erro: {ex.Message}");
            return Ok(); // Twilio quer 200 mesmo em caso de erro
        }
    }

    [HttpGet("webhook")]
    public IActionResult WebhookTest()
    {
        return Ok(new { message = "Webhook de WhatsApp está funcionando!" });
    }

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