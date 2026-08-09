using System;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class AccessPolicyDTO
{
    public Guid Id { get; set; }
    public Guid PolicyKey { get; set; }
    public int Version { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Feature { get; set; }
    public AccessScopeType? ScopeType { get; set; }
    public string? ScopeKey { get; set; }
    public AccessEffect Effect { get; set; }
    public AccessPolicyState State { get; set; }
}
