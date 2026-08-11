using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models;

// introduce-senior-portal §2 — configuração por instituição de um ModuleDefinition
// (habilitação, ordem, estado operacional, mensagem). InstitutionId é um Guid puro, sem FK
// de banco — mesma convenção já usada em Role/OrganizationalRole/ApplicationUser, onde o
// escopo institucional é reforçado pela aplicação (ICurrentUserContext), não pelo schema.
[Table("institutionmodule")]
public class InstitutionModule
{
    [Column("id")]
    public int Id { get; set; }

    [Column("institution_id")]
    public Guid InstitutionId { get; set; }

    [Column("module_definition_id")][ForeignKey("moduledefinition")]
    public int ModuleDefinitionId { get; set; }

    [Column("operational_state")]
    public OperationalState OperationalState { get; set; } = OperationalState.DISABLED;

    [Column("order")]
    public int Order { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; }

    [Column("operational_message")]
    public string? OperationalMessage { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    [JsonIgnore]
    public ModuleDefinition? ModuleDefinition { get; set; }
}
