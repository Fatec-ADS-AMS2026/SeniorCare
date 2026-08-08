namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class CarrierUpdateRequest : CarrierCreateRequest
{
    public uint RowVersion { get; set; }
}
