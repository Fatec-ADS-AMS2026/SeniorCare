namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class UnitOfMeasureUpdateRequest : UnitOfMeasureCreateRequest
{
    public uint RowVersion { get; set; }
}
