using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

// Parâmetros de paginação/filtro comuns aos GetAll dos catálogos (tarefa 3.5).
// Filtro é um "contém" case-insensitive sobre o(s) campo(s) de texto que cada
// controller considerar relevantes para busca — não há um campo genérico
// "nome" comum a todas as 9 entidades.
public class CatalogQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "page deve ser >= 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "pageSize deve estar entre 1 e 100.")]
    public int PageSize { get; set; } = 20;

    public string? Search { get; set; }

    // §9.1/9.2: listagens só trazem ativos por padrão — true pra incluir inativados.
    public bool IncludeInactive { get; set; } = false;
}
