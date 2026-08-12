using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class InstitutionModuleUpdateRequest
{
    public bool IsEnabled { get; set; }
    public int Order { get; set; }
    public OperationalState OperationalState { get; set; }
    public string? OperationalMessage { get; set; }
    public uint RowVersion { get; set; }
}
