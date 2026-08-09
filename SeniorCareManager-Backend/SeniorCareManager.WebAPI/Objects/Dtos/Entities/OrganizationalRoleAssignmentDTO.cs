using System;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class OrganizationalRoleAssignmentDTO
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrganizationalRoleId { get; set; }
    public AccessScopeType ScopeType { get; set; }
    public string? ScopeKey { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
