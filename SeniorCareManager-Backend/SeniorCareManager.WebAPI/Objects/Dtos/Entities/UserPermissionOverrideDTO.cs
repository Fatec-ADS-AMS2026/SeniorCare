using System;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class UserPermissionOverrideDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Feature { get; set; }
    public AccessScopeType? ScopeType { get; set; }
    public string? ScopeKey { get; set; }
    public AccessEffect Effect { get; set; }
    public string Justification { get; set; } = string.Empty;
    public Guid GrantedByUserId { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
