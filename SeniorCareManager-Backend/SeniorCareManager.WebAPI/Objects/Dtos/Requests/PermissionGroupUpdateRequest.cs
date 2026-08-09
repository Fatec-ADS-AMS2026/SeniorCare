namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class PermissionGroupUpdateRequest : PermissionGroupCreateRequest
{
    public uint RowVersion { get; set; }
}
