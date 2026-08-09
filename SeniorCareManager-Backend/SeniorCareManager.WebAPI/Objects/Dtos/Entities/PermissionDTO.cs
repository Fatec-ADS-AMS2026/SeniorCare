using System;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class PermissionDTO
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Feature { get; set; }
    public string? Description { get; set; }
    public bool IsSystemOperation { get; set; }
}
