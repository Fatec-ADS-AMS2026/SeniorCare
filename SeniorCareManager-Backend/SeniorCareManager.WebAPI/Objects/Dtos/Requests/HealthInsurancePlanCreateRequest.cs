using System.ComponentModel.DataAnnotations;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class HealthInsurancePlanCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    [EnumDataType(typeof(HealthPlanType))]
    public HealthPlanType Type { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Abbreviation { get; set; } = string.Empty;
}
