namespace SeniorCareManager.WebAPI.Objects.Enums
{
    // introduce-senior-portal §2 — estado operacional de um InstitutionModule. Valores
    // pinados explicitamente (armazenados como integer nativo, sem HasConversion — mesmo
    // padrão de AuditEventCategory) porque a migração adiciona um CHECK constraint no range
    // 0-3; reordenar ou remover um valor exige migração nova, não só recompilar.
    public enum OperationalState
    {
        AVAILABLE = 0,
        MAINTENANCE = 1,
        UNAVAILABLE = 2,
        DISABLED = 3
    }
}
