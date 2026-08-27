using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IIdentityNotificationService
{
    Task<NotificationDeliveryStatus> SendActivationAsync(
        ApplicationUser user,
        string token,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default);

    Task<NotificationDeliveryStatus> SendRecoveryAsync(
        ApplicationUser user,
        string token,
        CancellationToken cancellationToken = default);
}
