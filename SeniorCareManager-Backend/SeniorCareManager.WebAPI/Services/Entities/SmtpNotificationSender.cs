using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public sealed class SmtpNotificationSender : INotificationSender
{
    private readonly IConfiguration _configuration;
    private readonly ISmtpClientFactory _clientFactory;
    private readonly ILogger<SmtpNotificationSender> _logger;

    public SmtpNotificationSender(
        IConfiguration configuration,
        ISmtpClientFactory clientFactory,
        ILogger<SmtpNotificationSender> logger)
    {
        _configuration = configuration;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<NotificationDeliveryStatus> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("SMTP não configurado; notificação para {Recipient} não enviada.", recipient);
            return NotificationDeliveryStatus.Disabled;
        }

        try
        {
            var port = int.Parse(_configuration["Smtp:Port"]!, System.Globalization.CultureInfo.InvariantCulture);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromAddress = _configuration["Smtp:FromAddress"]!;
            var fromDisplayName = _configuration["Smtp:FromDisplayName"];
            var useStartTls = !bool.TryParse(_configuration["Smtp:UseStartTls"], out var parsed) || parsed;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromDisplayName ?? "SeniorCare", fromAddress));
            message.To.Add(MailboxAddress.Parse(recipient));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            await using var client = _clientFactory.Create();
            await client.ConnectAsync(
                host,
                port,
                useStartTls ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None,
                cancellationToken);
            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password!, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            return NotificationDeliveryStatus.Sent;
        }
        catch
        {
            // Não passe a exceção ao logger: alguns clientes SMTP incluem credenciais ou
            // conteúdo da sessão na mensagem/stack trace.
            _logger.LogWarning("Falha ao enviar notificação SMTP para {Recipient}.", recipient);
            return NotificationDeliveryStatus.Failed;
        }
    }
}
