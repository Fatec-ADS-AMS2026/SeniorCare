namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class PositionUpdateRequest : PositionCreateRequest
{
    public uint RowVersion { get; set; }
}
