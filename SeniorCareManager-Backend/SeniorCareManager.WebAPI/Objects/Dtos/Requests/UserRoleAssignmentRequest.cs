using System;
using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class UserRoleAssignmentRequest
{
    [Required]
    public Guid UserId { get; set; }
}
