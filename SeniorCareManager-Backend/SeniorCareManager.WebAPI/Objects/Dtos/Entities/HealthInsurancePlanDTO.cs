using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;
public class HealthInsurancePlanDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public HealthPlanType Type { get; set; }
    public string Abbreviation { get; set; } = string.Empty;
    public uint RowVersion { get; set; }
}
