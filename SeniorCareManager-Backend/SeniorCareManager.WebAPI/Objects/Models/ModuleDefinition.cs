using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SeniorCareManager.WebAPI.Objects.Models;

// introduce-senior-portal §2 — catálogo sistêmico dos módulos que o Senior Portal pode
// listar (não por instituição — a configuração por instituição é InstitutionModule).
// Nesta seção só é seedada via HasData (care/stock); não existe API que crie um
// ModuleDefinition novo em runtime (docs/architecture/senior-portal-contracts.md).
[Table("moduledefinition")]
public class ModuleDefinition
{
    [Column("id")]
    public int Id { get; set; }

    [Column("key")]
    public string Key { get; set; } = string.Empty;

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("icon")]
    public string Icon { get; set; } = string.Empty;

    [Column("path")]
    public string Path { get; set; } = string.Empty;

    [Column("required_permission_id")][ForeignKey("permission")]
    public Guid RequiredPermissionId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public Permission? RequiredPermission { get; set; }

    public ModuleDefinition() { }

    public ModuleDefinition(
        int id, string key, string name, string description, string icon, string path,
        Guid requiredPermissionId)
    {
        Id = id;
        Key = key;
        Name = name;
        Description = description;
        Icon = icon;
        Path = path;
        RequiredPermissionId = requiredPermissionId;
    }
}
