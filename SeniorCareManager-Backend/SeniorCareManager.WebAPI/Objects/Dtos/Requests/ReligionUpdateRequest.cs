namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

// Mesmo formato de ReligionCreateRequest hoje — tipo próprio porque tende a
// divergir (ex.: token de concorrência otimista só em update, tarefa 3.7).
public class ReligionUpdateRequest : ReligionCreateRequest
{
}
