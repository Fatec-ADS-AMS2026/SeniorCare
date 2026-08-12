using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

// introduce-senior-portal §3.2 — resposta mínima de GET /api/v1/me/modules
// (design.md decisão 5): só o que o portal precisa pra renderizar o catálogo, sem
// regras internas de autorização nem dados dos domínios.
public class ModuleCatalogItemDTO
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Order { get; set; }
    public OperationalState OperationalState { get; set; }
    public string? OperationalMessage { get; set; }
}
