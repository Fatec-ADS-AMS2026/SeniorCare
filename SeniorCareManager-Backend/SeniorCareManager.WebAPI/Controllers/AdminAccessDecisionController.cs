using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Explicação de decisão (§6.9) — "dados suficientes" é a camada determinante que o
// AccessDecisionService já produz (§5); nunca expõe a linha exata (override/política) que
// decidiu, evitando vazar detalhe interno de configuração.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminAccessDecisionController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly ICurrentUserContext _currentUserContext;

    public AdminAccessDecisionController(
        AppDbContext dbContext, IAccessDecisionService accessDecisionService, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
        _currentUserContext = currentUserContext;
    }

    [HttpPost("explain")]
    [RequirePermission("AccessDecisionExplanation", "read")]
    public async Task<ActionResult<AccessDecision>> Explain(AccessDecisionExplainRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var targetBelongsToInstitution = await _dbContext.Users
            .AnyAsync(u => u.Id == request.UserId && u.InstitutionId == institutionId);
        if (!targetBelongsToInstitution)
            throw new KeyNotFoundException("Conta não encontrada.");

        var decision = await _accessDecisionService.EvaluateAsync(
            request.UserId, request.Resource, request.Action, request.Feature, request.ScopeType, request.ScopeKey);
        return Ok(decision);
    }
}
