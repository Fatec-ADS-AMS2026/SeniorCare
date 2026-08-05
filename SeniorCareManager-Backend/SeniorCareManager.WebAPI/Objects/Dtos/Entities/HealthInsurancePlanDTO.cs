using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;
public class HealthInsurancePlanDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Type { get; set; }
    public string Abbreviation { get; set; }
}
