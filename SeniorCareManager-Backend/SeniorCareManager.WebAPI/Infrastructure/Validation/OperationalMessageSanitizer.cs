using System.Text.RegularExpressions;

namespace SeniorCareManager.WebAPI.Infrastructure.Validation;

// introduce-senior-portal §2.5 — valida InstitutionModule.OperationalMessage (a mensagem
// que aparece pro usuário quando um módulo está em MAINTENANCE/UNAVAILABLE). Guarda
// pragmática: rejeita HTML/URL (vetor técnico objetivo) e reusa o MESMO vocabulário de
// .github/scripts/check-clinical-scope.sh como denylist de termos clínicos/pessoais — não
// é a mesma ferramenta (aquele script escaneia código-fonte; este valida entrada em
// runtime), mas mantém consistente o que "escopo clínico/pessoal" significa neste projeto.
// Detecção semântica completa de conteúdo clínico/pessoal está fora do alcance de um
// denylist — isto é defesa razoável contra os casos óbvios, não uma garantia completa.
public static class OperationalMessageSanitizer
{
    private const int MaxLength = 280;

    private static readonly Regex HtmlTagPattern = new("[<>]", RegexOptions.Compiled);
    private static readonly Regex UrlPattern = new(@"https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Mesmo vocabulário de .github/scripts/check-clinical-scope.sh (FORBIDDEN_RE). O
    // marcador abaixo é o próprio denylist de rejeição, não escopo clínico entrando no
    // código — sem ele, o gate de CI se autoflagra por conter as palavras que ele mesmo
    // procura (falso positivo confirmado ao rodar o script contra este arquivo).
    private static readonly Regex ClinicalScopePattern = new(
        "prontu[aá]rio|medicalrecord|clinicaldata|dashboard|assinatura|e?signature|" + // clinical-scope:allow
        "profiss[aã]o|conselho de classe|conselho profissional", // clinical-scope:allow
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static IReadOnlyList<string> Validate(string? message)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(message))
            return errors;

        if (message.Length > MaxLength)
            errors.Add($"Mensagem operacional excede o limite de {MaxLength} caracteres.");

        if (HtmlTagPattern.IsMatch(message))
            errors.Add("Mensagem operacional não pode conter marcação HTML ('<' ou '>').");

        if (UrlPattern.IsMatch(message))
            errors.Add("Mensagem operacional não pode conter URL.");

        if (ClinicalScopePattern.IsMatch(message))
            errors.Add("Mensagem operacional não pode conter termo de escopo clínico ou pessoal.");

        return errors;
    }
}
