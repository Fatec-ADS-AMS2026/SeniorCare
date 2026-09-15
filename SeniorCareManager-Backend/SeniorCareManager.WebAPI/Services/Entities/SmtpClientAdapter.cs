using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public sealed class SmtpClientFactory : ISmtpClientFactory
{
    public ISmtpClientAdapter Create() => new SmtpClientAdapter(new SmtpClient());
}

internal sealed class SmtpClientAdapter : ISmtpClientAdapter
{
    private readonly SmtpClient _client;

    public SmtpClientAdapter(SmtpClient client)
    {
        _client = client;
    }

    public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken) =>
        _client.ConnectAsync(host, port, options, cancellationToken);

    public Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken) =>
        _client.AuthenticateAsync(username, password, cancellationToken);

    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken) =>
        _ = await _client.SendAsync(message, cancellationToken);

    public Task DisconnectAsync(bool quit, CancellationToken cancellationToken) =>
        _client.DisconnectAsync(quit, cancellationToken);

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }
}
