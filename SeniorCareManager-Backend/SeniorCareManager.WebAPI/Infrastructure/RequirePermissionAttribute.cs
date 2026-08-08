using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

        if (!decision.Allowed)
            throw new AccessDeniedException($"Acesso negado para {_resource}/{_action}.");
    }
}
