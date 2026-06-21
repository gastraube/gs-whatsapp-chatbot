using gschatbot.api.Domain.Interfaces;
using gschatbot.api.Models;
using System.Globalization;

namespace gschatbot.api.Services.Handlers;

[IntentHandler("confirmar")]
public class ConfirmarHandler : IIntentHandler
{
    private readonly IEspecialistaRepository _especialistaRepo;
    private readonly IHorarioConsultaRepository _horarioRepo;
    private readonly IAgendamentoRepository _agendamentoRepo;
    private readonly IClienteRepository _clienteRepo;
    private readonly IHistoricoMensagemRepository _historicoRepo;
    private readonly IPlanoAssistenciaRepository _planoRepo;
    private readonly INotificacaoService _notificacao;
    private readonly ILogger<ConfirmarHandler> _logger;

    public ConfirmarHandler(
        IEspecialistaRepository especialistaRepo,
        IHorarioConsultaRepository horarioRepo,
        IAgendamentoRepository agendamentoRepo,
        IClienteRepository clienteRepo,
        IHistoricoMensagemRepository historicoRepo,
        IPlanoAssistenciaRepository planoRepo,
        INotificacaoService notificacao,
        ILogger<ConfirmarHandler> logger)
    {
        _especialistaRepo = especialistaRepo;
        _horarioRepo = horarioRepo;
        _agendamentoRepo = agendamentoRepo;
        _clienteRepo = clienteRepo;
        _historicoRepo = historicoRepo;
        _planoRepo = planoRepo;
        _notificacao = notificacao;
        _logger = logger;
    }

    public async Task Handle(int clienteId, string numeroWhatsApp, LlmResponse llmResponse)
    {
        try
        {
            var dados = llmResponse.Dados;

            var medicoNome = dados.TryGetValue("medico", out var m) ? m?.ToString() : null;
            var dataStr = dados.TryGetValue("data", out var d) ? d?.ToString() : null;
            var horaStr = dados.TryGetValue("hora", out var h) ? h?.ToString() : null;
            var tipoPagamento = (dados.TryGetValue("tipo_pagamento", out var tp) ? tp?.ToString() : null) ?? "particular";
            var planoNome = dados.TryGetValue("plano", out var pl) ? pl?.ToString() : null;
            var nomeCliente = dados.TryGetValue("nome_cliente", out var nc) ? nc?.ToString() : null;
            var emailCliente = dados.TryGetValue("email_cliente", out var ec) ? ec?.ToString() : null;
            var cpfCliente = dados.TryGetValue("cpf_cliente", out var cpf) ? cpf?.ToString() : null;
            var dataNascStr = dados.TryGetValue("data_nascimento", out var dn) ? dn?.ToString() : null;

            if (string.IsNullOrEmpty(medicoNome) || string.IsNullOrEmpty(dataStr) || string.IsNullOrEmpty(horaStr))
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, llmResponse.Resposta);
                return;
            }

            if (!DateOnly.TryParseExact(dataStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data) ||
                !TimeOnly.TryParseExact(horaStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Não consegui identificar a data ou hora. Pode confirmar qual horário você escolheu?");
                return;
            }

            var especialista = await _especialistaRepo.BuscarPorNomeAsync(medicoNome);
            if (especialista == null)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, $"Não encontrei o médico '{medicoNome}'. Pode tentar novamente?");
                return;
            }

            // Atualiza dados do cliente se vieram preenchidos
            var cliente = await _clienteRepo.ObterOuCriarAsync(numeroWhatsApp);
            var dadosAtualizados = false;

            if (!string.IsNullOrEmpty(nomeCliente) && string.IsNullOrEmpty(cliente.Nome))
            { cliente.Nome = nomeCliente; dadosAtualizados = true; }
            if (!string.IsNullOrEmpty(emailCliente) && string.IsNullOrEmpty(cliente.Email))
            { cliente.Email = emailCliente; dadosAtualizados = true; }
            if (!string.IsNullOrEmpty(cpfCliente) && string.IsNullOrEmpty(cliente.Cpf))
            { cliente.Cpf = cpfCliente; dadosAtualizados = true; }
            if (!string.IsNullOrEmpty(dataNascStr) && cliente.DataNascimento == null &&
                DateTime.TryParseExact(dataNascStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dataNasc))
            { cliente.DataNascimento = dataNasc; dadosAtualizados = true; }

            if (dadosAtualizados)
                await _clienteRepo.AtualizarDadosAsync(cliente);

            // Verifica se falta alguma informação obrigatória
            var faltando = new List<string>();
            if (string.IsNullOrEmpty(cliente.Nome)) faltando.Add("nome completo");
            if (string.IsNullOrEmpty(cliente.Email)) faltando.Add("e-mail");
            if (string.IsNullOrEmpty(cliente.Cpf)) faltando.Add("CPF");
            if (cliente.DataNascimento == null) faltando.Add("data de nascimento");

            if (faltando.Count > 0)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp,
                    $"Para confirmar o agendamento preciso de alguns dados: {string.Join(", ", faltando)}.");
                return;
            }

            PlanoAssistencia? plano = null;
            if (tipoPagamento == "plano" && !string.IsNullOrEmpty(planoNome))
            {
                plano = await _planoRepo.BuscarPorNomeAsync(planoNome);
                if (plano == null)
                {
                    await _notificacao.EnviarMensagemAsync(numeroWhatsApp, $"Não encontrei o plano '{planoNome}'. Pode verificar o nome ou prefere pagar particular?");
                    return;
                }

                var aceitaPlano = await _planoRepo.EspecialistaAceitaPlanoAsync(especialista.Id, plano.Id);
                if (!aceitaPlano)
                {
                    await _notificacao.EnviarMensagemAsync(numeroWhatsApp, $"{especialista.Nome} não atende pelo plano {plano.Nome}. Gostaria de agendar como particular?");
                    return;
                }
            }

            var slot = await _horarioRepo.BuscarPorDetalhesAsync(especialista.Id, data, hora);
            if (slot == null)
            {
                await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Esse horário não está mais disponível. Gostaria de ver outros horários?");
                return;
            }

            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                EspecialistaId = especialista.Id,
                HorarioConsultaId = slot.Id,
                Status = StatusAgendamento.Pendente,
                TipoPagamento = tipoPagamento,
                PlanoAssistenciaId = plano?.Id
            };

            await _agendamentoRepo.CriarAsync(agendamento);
            await _horarioRepo.ReservarAsync(slot);

            var pagamentoInfo = plano != null ? $"Plano: {plano.Nome}" : "Pagamento: Particular";

            var resposta = $"✅ Agendamento realizado!\n\n" +
                $"📅 Data: {slot.DataConsulta:dd/MM/yyyy}\n" +
                $"🕐 Hora: {slot.HoraInicio:HH:mm}h\n" +
                $"👨‍⚕️ Médico: {slot.Especialista.Nome}\n" +
                $"🏥 Especialidade: {slot.Especialista.Especialidade.Nome}\n" +
                $"📍 Local: {slot.Endereco.Rua}, {slot.Endereco.Numero} - {slot.Endereco.Bairro}, {slot.Endereco.Cidade}\n" +
                $"💳 {pagamentoInfo}\n\n" +
                $"⏰ 24h antes da consulta enviaremos uma mensagem solicitando sua confirmação.";

            await _historicoRepo.AdicionarAsync(new HistoricoMensagem
            {
                ClienteId = clienteId,
                AgendamentoId = agendamento.Id,
                RemetenteId = "bot",
                Mensagem = resposta,
                Tipo = "agendamento"
            });

            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, resposta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ConfirmarHandler] Erro ao confirmar agendamento");
            await _notificacao.EnviarMensagemAsync(numeroWhatsApp, "Erro ao confirmar o agendamento. Tente novamente.");
        }
    }
}
