namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ManufacturerUpdateRequest : ManufacturerCreateRequest
{
    public uint RowVersion { get; set; }
}
