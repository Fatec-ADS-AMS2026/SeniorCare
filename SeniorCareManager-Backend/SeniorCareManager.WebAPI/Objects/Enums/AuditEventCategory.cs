namespace SeniorCareManager.WebAPI.Objects.Enums
{
    // As 5 categorias da §8.1 — autenticação, sessão, configuração, catálogo e decisão de
    // acesso. O subtipo exato do evento fica em AuditEvent.Resource/Action (mesmo vocabulário
    // de Permission/RequirePermissionAttribute, §5), não num enum por tipo de evento.
    public enum AuditEventCategory
    {
        AUTHENTICATION = 1,
        SESSION = 2,
        CONFIGURATION = 3,
        CATALOG = 4,
        ACCESS_DECISION = 5
    }
}
