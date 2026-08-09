using System.Collections.Generic;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class MfaConfirmResponse
{
    // Devolvidos uma única vez — nunca mais recuperáveis em claro depois desta resposta.
    public List<string> RecoveryCodes { get; set; } = new();

    // Preenchido quando a confirmação também completou um login pendente (veio de
    // ChallengeToken de MFA_ENROLLMENT) — nulo em cadastro voluntário com sessão já ativa.
    public CurrentIdentityDTO? Identity { get; set; }
}
