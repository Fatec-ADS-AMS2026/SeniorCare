using System.Text.RegularExpressions;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Infrastructure.Validation;

// introduce-senior-portal §2.4 — guarda de regressão sobre o seed de ModuleDefinition
// (docs/architecture/senior-portal-contracts.md) e validador pronto pra uma eventual API
// administrativa de criação (fora do escopo das 55 tarefas atuais, mas o cenário "Destino
// externo não autorizado"/"Identificador duplicado" do spec.md pressupõe essa validação
// existir). Allowlist, não denylist — um destino/ícone novo exige código novo, não é
// aceito por texto livre (design.md decisão 4, alternativa "registro livre" rejeitada).
public static class ModuleDefinitionValidator
{
    private static readonly Regex KeyPattern = new("^[a-z][a-z0-9-]{1,49}$", RegexOptions.Compiled);

    // Nomes reais do pacote @phosphor-icons/react já usado pelos dois front-ends
    // (docs/architecture/senior-portal-contracts.md §1) — estender aqui quando um novo
    // módulo aprovado precisar de um ícone ainda não usado.
    private static readonly HashSet<string> ApprovedIcons = new(StringComparer.Ordinal)
    {
        "HeartStraight",
        "Package",
    };

    // Prefixos de caminho do contrato de rotas de §1 — só módulos publicados sob a mesma
    // origem, nunca URL absoluta ou origem externa.
    private static readonly string[] AllowedPathPrefixes = { "/care", "/stock" };

    public static IReadOnlyList<string> Validate(ModuleDefinition candidate, IReadOnlySet<Guid> existingPermissionIds)
    {
        var errors = new List<string>();

        if (!KeyPattern.IsMatch(candidate.Key))
            errors.Add($"Key '{candidate.Key}' não corresponde ao padrão exigido (minúsculas, dígitos e hífen, começando por letra).");

        if (!ApprovedIcons.Contains(candidate.Icon))
            errors.Add($"Icon '{candidate.Icon}' não está na allowlist de ícones aprovados.");

        if (!IsAllowedRelativePath(candidate.Path))
            errors.Add($"Path '{candidate.Path}' não é um caminho relativo permitido (deve começar com {string.Join(" ou ", AllowedPathPrefixes)}).");

        if (!existingPermissionIds.Contains(candidate.RequiredPermissionId))
            errors.Add($"RequiredPermissionId '{candidate.RequiredPermissionId}' não corresponde a nenhuma Permission existente.");

        return errors;
    }

    private static bool IsAllowedRelativePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        // Rejeita esquema (http://, https://, //origem-relativa-a-protocolo) e qualquer
        // coisa que não comece com uma barra única — mesma regra do contrato de returnTo.
        if (!path.StartsWith('/') || path.StartsWith("//")) return false;
        if (path.Contains("://")) return false;

        return AllowedPathPrefixes.Any(prefix => path == prefix || path.StartsWith(prefix + "/", StringComparison.Ordinal));
    }
}
