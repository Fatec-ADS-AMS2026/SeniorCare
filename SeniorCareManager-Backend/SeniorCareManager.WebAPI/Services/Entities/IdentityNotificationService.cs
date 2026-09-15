using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public sealed class IdentityNotificationService : IIdentityNotificationService
{
    private readonly INotificationSender _sender;
    private readonly IAuditService _auditService;
    private readonly IConfiguration _configuration;

    public IdentityNotificationService(
        INotificationSender sender,
        IAuditService auditService,
        IConfiguration configuration)
    {
        _sender = sender;
        _auditService = auditService;
        _configuration = configuration;
    }

    public Task<NotificationDeliveryStatus> SendActivationAsync(
        ApplicationUser user,
        string token,
        Guid? actorUserId = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(user, token, "Activation", "Ative sua conta no SeniorCare", actorUserId ?? user.Id, cancellationToken);

    public Task<NotificationDeliveryStatus> SendRecoveryAsync(
        ApplicationUser user,
        string token,
        CancellationToken cancellationToken = default) =>
        SendAsync(user, token, "Recovery", "Recupere seu acesso ao SeniorCare", user.Id, cancellationToken);

    private async Task<NotificationDeliveryStatus> SendAsync(
        ApplicationUser user,
        string token,
        string notificationType,
        string subject,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["Frontend:ActivationBaseUrl"] ?? string.Empty;
        var targetUrl = notificationType == "Recovery" ? RecoveryUrl(baseUrl) : baseUrl;
        var link = QueryHelpers.AddQueryString(targetUrl, new System.Collections.Generic.Dictionary<string, string?>
        {
            ["email"] = user.Email,
            ["token"] = token,
        });
        var body = notificationType == "Recovery"
            ? $"Use o link abaixo para definir uma nova senha. O link é de uso único e expira.\n\n{link}"
            : $"Use o link abaixo para ativar sua conta e definir sua senha. O link é de uso único e expira.\n\n{link}";

        var status = await _sender.SendAsync(user.Email!, subject, body, cancellationToken);
        if (status != NotificationDeliveryStatus.Disabled)
        {
            await _auditService.RecordAsync(
                AuditEventCategory.AUTHENTICATION,
                "IdentityNotification",
                notificationType,
                status == NotificationDeliveryStatus.Sent ? AuditOutcome.SUCCESS : AuditOutcome.FAILURE,
                actorUserId: actorUserId,
                institutionId: user.InstitutionId,
                targetUserId: user.Id,
                afterValue: new { Recipient = user.Email, NotificationType = notificationType },
                description: status == NotificationDeliveryStatus.Sent ? "Notificação enviada." : "Falha de entrega.",
                cancellationToken: cancellationToken);
        }

        return status;
    }

    private static string RecoveryUrl(string activationBaseUrl)
    {
        if (!Uri.TryCreate(activationBaseUrl, UriKind.Absolute, out var activationUri))
            return activationBaseUrl;

        var builder = new UriBuilder(activationUri) { Path = "/redefinir-senha", Query = string.Empty };
        return builder.Uri.ToString();
    }
}
