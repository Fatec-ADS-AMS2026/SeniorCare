using System;
using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class PermissionLinkRequest
{
    [Required]
    public Guid PermissionId { get; set; }
}
