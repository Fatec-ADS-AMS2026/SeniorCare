namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class HealthInsurancePlanUpdateRequest : HealthInsurancePlanCreateRequest
{
    public uint RowVersion { get; set; }
}
