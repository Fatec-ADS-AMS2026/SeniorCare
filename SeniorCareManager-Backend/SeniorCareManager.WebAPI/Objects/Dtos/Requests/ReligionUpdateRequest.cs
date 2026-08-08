namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ReligionUpdateRequest : ReligionCreateRequest
{
    // RowVersion lido no GET anterior — usado como token de concorrência otimista
    // (tarefa 3.7). Se a linha mudou desde então, a atualização retorna 409.
    public uint RowVersion { get; set; }
}
