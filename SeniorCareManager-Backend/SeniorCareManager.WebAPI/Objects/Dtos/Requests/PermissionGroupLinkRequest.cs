using System;
using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

// Reusado por Role e OrganizationalRole para anexar/remover um PermissionGroup.
public class PermissionGroupLinkRequest
{
    [Required]
    public Guid PermissionGroupId { get; set; }
}
