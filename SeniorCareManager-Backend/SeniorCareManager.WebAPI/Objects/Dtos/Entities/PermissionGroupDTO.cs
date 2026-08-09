using System;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class PermissionGroupDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint RowVersion { get; set; }
}
