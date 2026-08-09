using System;
using System.ComponentModel.DataAnnotations;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class OrganizationalRoleAssignmentCreateRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid OrganizationalRoleId { get; set; }

    [Required]
    public AccessScopeType ScopeType { get; set; }

    public string? ScopeKey { get; set; }

    [Required]
    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }
}
