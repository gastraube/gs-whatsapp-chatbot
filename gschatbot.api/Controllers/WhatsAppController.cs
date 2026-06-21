using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using gschatbot.api.Services.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace gschatbot.api.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppController : ControllerBase
{
    private readonly ILlmService _llmService;
    private readonly INotificacaoService _notificacao;
    private readonly IClienteRepository _clienteRepo;
    private readonly IEspecialidadeRepository _especialidadeRepo;
    private readonly IEspecialistaRepository _especialistaRepo;
    private readonly IHistoricoMensagemRepository _historicoRepo;
    private readonly IPlanoAssistenciaRepository _planoRepo;
    private readonly IMetodoPagamentoRepository _metodoPagamentoRepo;
    private readonly IntentDispatcher _intentDispatcher;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        ILlmService llmService,
        INotificacaoService notificacao,
        IClienteRepository clienteRepo,
        IEspecialidadeRepository especialidadeRepo,
        IEspecialistaRepository especialistaRepo,
        IHistoricoMensagemRepository historicoRepo,
        IPlanoAssistenciaRepository planoRepo,
        IMetodoPagamentoRepository metodoPagamentoRepo,
        IntentDispatcher intentDispatcher,
        ILogger<WhatsAppController> logger)
    {
        _llmService = llmService;
        _notificacao = notificacao;
        _clienteRepo = clienteRepo;
        _especialidadeRepo = especialidadeRepo;
        _especialistaRepo = especialistaRepo;
        _historicoRepo = historicoRepo;
        _planoRepo = planoRepo;
        _metodoPagamentoRepo = metodoPagamentoRepo;
        _intentDispatcher = intentDispatcher;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceberMensagem([FromForm] IFormCollection form)
    {
        try
        {
            var fromNumber = form["From"].ToString();
            var messageBody = form["Body"].ToString();

            _logger.LogInformation("[WhatsAppController] Recebido de {From}: {Body}", fromNumber, messageBody);

            var numeroLimpo = fromNumber.Replace("whatsapp:", "").Replace("+", "").Replace(" ", "");
            var cliente = await _clienteRepo.ObterOuCriarAsync(numeroLimpo);

            var historico = await _historicoRepo.ListarRecentesAsync(cliente.Id, 10);
            var historicoTuples = historico
                .Select(h => (h.RemetenteId == "cliente" ? "Cliente" : "Bot", h.Mensagem))
                .ToList();

            var especialidades = await _especialidadeRepo.ListarNomesAsync();
            var nomesEspecialistas = await _especialistaRepo.ListarNomesAtivosAsync();
            var planos = await _planoRepo.ListarNomesAtivosAsync();
            var planosCliente = await _planoRepo.ListarPlanosClienteAsync(cliente.Id);

            var todosEspecialistas = await _especialistaRepo.ListarAtivosAsync();
            var metodosPagamento = new Dictionary<string, List<string>>();
            foreach (var e in todosEspecialistas)
            {
                var metodos = await _metodoPagamentoRepo.ListarNomesPorEspecialistaAsync(e.Id);
                if (metodos.Count > 0)
                    metodosPagamento[e.Nome] = metodos;
            }

            var llmResponse = await _llmService.ProcessarMensagemAsync(messageBody, historicoTuples, especialidades, nomesEspecialistas, planos, planosCliente, metodosPagamento);

            await _historicoRepo.AdicionarAsync(new HistoricoMensagem
            {
                ClienteId = cliente.Id,
                RemetenteId = "cliente",
                Mensagem = messageBody,
                Tipo = "texto"
            });

            var handled = await _intentDispatcher.Dispatch(cliente.Id, numeroLimpo, llmResponse);

            if (!handled)
            {
                await _notificacao.EnviarMensagemAsync(numeroLimpo, llmResponse.Resposta);
                await _historicoRepo.AdicionarAsync(new HistoricoMensagem
                {
                    ClienteId = cliente.Id,
                    RemetenteId = "bot",
                    Mensagem = llmResponse.Resposta,
                    Tipo = "texto"
                });
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WhatsAppController] Erro ao processar mensagem");
            return Ok();
        }
    }

    [HttpGet("webhook")]
    public IActionResult WebhookTest() => Ok(new { message = "Webhook de WhatsApp está funcionando!" });

    [HttpGet("clientes")]
    public async Task<IActionResult> ListarClientes()
    {
        var clientes = await _clienteRepo.ListarComAgendamentosAsync();
        return Ok(clientes);
    }

    [HttpGet("clientes/{id}/historico")]
    public async Task<IActionResult> HistoricoCliente(int id)
    {
        var historico = await _historicoRepo.ListarPorClienteAsync(id);
        return Ok(historico);
    }
}
