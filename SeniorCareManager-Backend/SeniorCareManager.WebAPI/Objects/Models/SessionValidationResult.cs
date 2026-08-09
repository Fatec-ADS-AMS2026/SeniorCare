namespace SeniorCareManager.WebAPI.Objects.Models
{
    public enum SessionValidationOutcome
    {
        Valid,
        Rotated,
        Reused,
        Rejected
    }

    public class SessionValidationResult
    {
        public SessionValidationOutcome Outcome { get; init; }

        // Só preenchido quando Outcome == Rotated — a nova chave em claro, pra reconstruir o
        // cookie. Nunca persistida em claro (só o hash vai pro banco).
        public string? NewRawKey { get; init; }
    }
}
