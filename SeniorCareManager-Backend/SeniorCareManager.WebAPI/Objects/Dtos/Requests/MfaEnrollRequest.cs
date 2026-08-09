namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

// Sem ChallengeToken = cadastro voluntário por quem já tem sessão autenticada; com
// ChallengeToken = cadastro obrigatório antes de existir sessão (§7.7).
public class MfaEnrollRequest
{
    public string? ChallengeToken { get; set; }
}
