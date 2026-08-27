using FluentAssertions;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.UnitTests.Services;

public sealed class SmtpNotificationSenderTests
{
    [Fact]
    public async Task SendAsync_WithoutSmtpConfiguration_ReturnsDisabledWithoutCreatingClient()
    {
        var factory = new Mock<ISmtpClientFactory>(MockBehavior.Strict);
        var sender = CreateSender(new Dictionary<string, string?>(), factory.Object);

        var result = await sender.SendAsync("admin@example.com", "Assunto", "corpo sensível");

        result.Should().Be(NotificationDeliveryStatus.Disabled);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SendAsync_WithConfiguration_SendsMessage()
    {
        var client = new FakeSmtpClient();
        var factory = new Mock<ISmtpClientFactory>();
        factory.Setup(x => x.Create()).Returns(client);
        var sender = CreateSender(ValidConfiguration(), factory.Object);

        var result = await sender.SendAsync("admin@example.com", "Ativação", "conteúdo");

        result.Should().Be(NotificationDeliveryStatus.Sent);
        client.Message.Should().NotBeNull();
        client.Message!.Subject.Should().Be("Ativação");
        client.Authenticated.Should().BeTrue();
        client.Disconnected.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WhenClientFails_ReturnsFailedWithoutPropagating()
    {
        var client = new FakeSmtpClient { FailOnConnect = true };
        var factory = new Mock<ISmtpClientFactory>();
        factory.Setup(x => x.Create()).Returns(client);
        var sender = CreateSender(ValidConfiguration(), factory.Object);

        var result = await sender.SendAsync("admin@example.com", "Ativação", "token-secreto");

        result.Should().Be(NotificationDeliveryStatus.Failed);
    }

    private static SmtpNotificationSender CreateSender(
        IDictionary<string, string?> values,
        ISmtpClientFactory factory)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new SmtpNotificationSender(
            configuration,
            factory,
            Mock.Of<ILogger<SmtpNotificationSender>>());
    }

    private static Dictionary<string, string?> ValidConfiguration() => new()
    {
        ["Smtp:Host"] = "smtp.example.test",
        ["Smtp:Port"] = "587",
        ["Smtp:Username"] = "user",
        ["Smtp:Password"] = "password-for-test", // gitleaks:allow — valor exclusivamente de teste
        ["Smtp:FromAddress"] = "noreply@example.test",
        ["Smtp:FromDisplayName"] = "SeniorCare",
        ["Smtp:UseStartTls"] = "true",
    };

    private sealed class FakeSmtpClient : ISmtpClientAdapter
    {
        public bool FailOnConnect { get; init; }
        public bool Authenticated { get; private set; }
        public bool Disconnected { get; private set; }
        public MimeMessage? Message { get; private set; }

        public Task ConnectAsync(string host, int port, SecureSocketOptions options, CancellationToken cancellationToken)
        {
            if (FailOnConnect) throw new InvalidOperationException("falha sensível simulada");
            return Task.CompletedTask;
        }

        public Task AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
        {
            Authenticated = true;
            return Task.CompletedTask;
        }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            Message = message;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(bool quit, CancellationToken cancellationToken)
        {
            Disconnected = true;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
