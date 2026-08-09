using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Infrastructure;

// Autorização fina por endpoint (spec §5, tarefa 5.10). Roda depois do AuthorizeFilter
// global (escopo Action é avaliado após escopo Global pelo MVC), então quando este filtro
// executa a requisição já passou pela checagem de autenticação (401 já teria interrompido
// o pipeline antes daqui).
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string _resource;
    private readonly string _action;
    private readonly string? _feature;

    public RequirePermissionAttribute(string resource, string action, string? feature = null)
    {
        _resource = resource;
        _action = action;
        _feature = feature;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new AccessDeniedException("Identidade autenticada sem identificador válido.");

        var accessDecisionService = context.HttpContext.RequestServices.GetRequiredService<IAccessDecisionService>();
        var decision = await accessDecisionService.EvaluateAsync(userId, _resource, _action, _feature);

        // Ponto único de "decisão protegida" (§8.5): este filtro é o que efetivamente gateia
        // uma ação administrativa — diferente de AccessDecisionService.EvaluateAsync sendo
        // usado por BuildCurrentIdentityAsync pra listar todas as permissões efetivas em
        // todo /me, o que inundaria a auditoria sem necessidade. Cobre "uso de SYSTEM_ADMIN"
        // de graça, já que é um dos valores possíveis de DeterminingLayer.
        var auditService = context.HttpContext.RequestServices.GetRequiredService<IAuditService>();
        var currentUserContext = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserContext>();
        await auditService.RecordAsync(
            AuditEventCategory.ACCESS_DECISION,
            _resource,
            _action,
            decision.Allowed ? AuditOutcome.SUCCESS : AuditOutcome.DENIED,
            actorUserId: userId,
            institutionId: await currentUserContext.GetInstitutionIdAsync(),
            feature: _feature,
            determiningLayer: decision.DeterminingLayer);

        if (!decision.Allowed)
            throw new AccessDeniedException($"Acesso negado para {_resource}/{_action}.");
    }
}
