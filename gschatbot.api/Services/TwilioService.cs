using gschatbot.api.Data;
using gschatbot.api.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Microsoft.EntityFrameworkCore;

namespace gschatbot.api.Services;

public class TwilioService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public TwilioService(IConfiguration config, AppDbContext context)
    {
        _config = config;
        _context = context;

        var accountSid = _config["Twilio:AccountSid"];
        var authToken = _config["Twilio:AuthToken"];
        TwilioClient.Init(accountSid, authToken);
    }

    public async Task SendMessage(string toNumber, string messageBody)
    {
        try
        {
            var fromNumber = _config["Twilio:PhoneNumber"] ?? throw new InvalidOperationException("Twilio:PhoneNumber não configurado.");

            var from = fromNumber.StartsWith("whatsapp:") ? fromNumber : $"whatsapp:{fromNumber}";
            var to = $"whatsapp:+{toNumber.Replace("whatsapp:", "").TrimStart('+')}";

            var message = await MessageResource.CreateAsync(
                body: messageBody,
                from: new PhoneNumber(from),
                to: new PhoneNumber(to)
            );

            Console.WriteLine($"[TwilioService] Mensagem enviada: {message.Sid}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TwilioService] Erro ao enviar: {ex.Message}");
            throw;
        }
    }

    public async Task<Cliente> GetOrCreateCliente(string numeroWhatsApp, string nome = null)
    {
        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(c => c.NumeroWhatsApp == numeroWhatsApp);

        if (cliente == null)
        {
            cliente = new Cliente
            {
                NumeroWhatsApp = numeroWhatsApp,
                Nome = nome ?? "Cliente",
                Ativo = true
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Cria sessão de conversa
            var sessao = new SessaoConversa
            {
                ClienteId = cliente.Id,
                EstadoAtual = "inicial"
            };

            _context.SessoesConversa.Add(sessao);
            await _context.SaveChangesAsync();
        }

        return cliente;
    }

    public async Task SaveHistoricoMensagem(int clienteId, int? agendamentoId, string remetenteId, string mensagem)
    {
        var historico = new HistoricoMensagem
        {
            ClienteId = clienteId,
            AgendamentoId = agendamentoId,
            RemetenteId = remetenteId,
            Mensagem = mensagem,
            Tipo = "texto"
        };

        _context.HistoricoMensagens.Add(historico);
        await _context.SaveChangesAsync();
    }
}