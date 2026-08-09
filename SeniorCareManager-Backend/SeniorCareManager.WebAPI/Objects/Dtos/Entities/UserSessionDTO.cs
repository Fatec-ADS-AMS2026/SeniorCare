using System;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class UserSessionDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}
