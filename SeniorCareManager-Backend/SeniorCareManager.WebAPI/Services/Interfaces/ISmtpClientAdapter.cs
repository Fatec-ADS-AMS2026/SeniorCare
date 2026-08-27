using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Security;
using MimeKit;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface ISmtpClientAdapter : IAsyncDisposable
{
    Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken);
    Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken);
    Task DisconnectAsync(bool quit, CancellationToken cancellationToken);
}

public interface ISmtpClientFactory
{
    ISmtpClientAdapter Create();
}
