namespace SeniorCareManager.WebAPI.Infrastructure;

// Lançada pelo RequirePermissionAttribute quando AccessDecisionService nega a operação —
// mapeada para 403. Distinta de "não autenticado" (401, tratado pelo middleware de
// autenticação antes de qualquer controller rodar).
public class AccessDeniedException : Exception
{
    public AccessDeniedException(string message) : base(message)
    {
    }
}
