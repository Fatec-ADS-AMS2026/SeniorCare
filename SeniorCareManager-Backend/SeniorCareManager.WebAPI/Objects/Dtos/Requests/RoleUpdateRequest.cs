namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class RoleUpdateRequest : RoleCreateRequest
{
    public uint RowVersion { get; set; }
}
