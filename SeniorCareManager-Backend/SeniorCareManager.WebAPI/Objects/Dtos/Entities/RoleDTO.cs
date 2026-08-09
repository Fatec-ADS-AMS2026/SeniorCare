using System;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class RoleDTO
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint RowVersion { get; set; }
}
