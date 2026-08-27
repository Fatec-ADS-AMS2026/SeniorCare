using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

public sealed class TestNotificationSender : INotificationSender
{
    public NotificationDeliveryStatus Status { get; set; } = NotificationDeliveryStatus.Disabled;
    public List<(string Recipient, string Subject, string Body)> Messages { get; } = new();

    public Task<NotificationDeliveryStatus> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        Messages.Add((recipient, subject, body));
        return Task.FromResult(Status);
    }

    public void Reset(NotificationDeliveryStatus status)
    {
        Messages.Clear();
        Status = status;
    }
}
