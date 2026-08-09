using System;
using System.Collections.Generic;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class CurrentIdentityDTO
{
    public Guid UserId { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<OrganizationalResponsibilityDTO> OrganizationalResponsibilities { get; set; } = new();
    public List<EffectivePermissionDTO> EffectivePermissions { get; set; } = new();
}

public class OrganizationalResponsibilityDTO
{
    public string Name { get; set; } = string.Empty;
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeKey { get; set; }
}

public class EffectivePermissionDTO
{
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Feature { get; set; }
}
