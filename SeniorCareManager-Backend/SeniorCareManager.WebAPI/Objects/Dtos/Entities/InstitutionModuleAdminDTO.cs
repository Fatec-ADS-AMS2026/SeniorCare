using System;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

// introduce-senior-portal §3.3 — visão administrativa de um InstitutionModule. Inclui
// RowVersion (concorrência otimista, §3.4) e os identificadores/nome do ModuleDefinition
// associado, já que o admin nunca cria/exclui InstitutionModule diretamente (provisionado
// de forma idempotente, §2.3) — só consulta e altera habilitação/ordem/estado/mensagem.
public class InstitutionModuleAdminDTO
{
    public int Id { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int Order { get; set; }
    public OperationalState OperationalState { get; set; }
    public string? OperationalMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public uint RowVersion { get; set; }
}
