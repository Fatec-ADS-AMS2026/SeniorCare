using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface INotificationSender
{
    Task<NotificationDeliveryStatus> SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
