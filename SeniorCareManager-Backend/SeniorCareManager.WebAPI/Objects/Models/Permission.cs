using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Vocabulário fixo do sistema — não delimitado por instituição. É o que o código
    // realmente expõe (recurso/ação/funcionalidade), não algo que uma ILPI inventa.
    [Table("permission")]
    public class Permission
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("resource")]
        public string Resource { get; set; } = string.Empty;

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("feature")]
        public string? Feature { get; set; }

        // Marca as permissões que o bypass de SYSTEM_ADMIN pode usar — reservado a
        // operações de sistema, nunca a dados de negócio.
        [Column("is_system_operation")]
        public bool IsSystemOperation { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public Permission() { }

        public Permission(Guid id, string resource, string action, string? description = null, bool isSystemOperation = false)
        {
            Id = id;
            Resource = resource;
            Action = action;
            Description = description;
            IsSystemOperation = isSystemOperation;
        }
    }
}
