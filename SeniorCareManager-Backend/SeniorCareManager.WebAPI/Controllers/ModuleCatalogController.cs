using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// introduce-senior-portal §3.2. Rota literal (não [controller]) porque o contrato do
// design.md fixa exatamente `GET /api/v1/me/modules` — não é um recurso [controller]
// convencional. Sem [RequirePermission]: como em AuthController.GetMe, ver o próprio
// catálogo não é gated por uma permissão específica — a filtragem por permissão efetiva
// acontece dentro do serviço, por módulo (spec.md "Descoberta de módulos usa permissões
// efetivas").
[ApiController]
[Route("api/v1/me/modules")]
public class ModuleCatalogController : ControllerBase
{
    private readonly IModuleCatalogService _moduleCatalogService;
    private readonly ILogger<ModuleCatalogController> _logger;

    public ModuleCatalogController(IModuleCatalogService moduleCatalogService, ILogger<ModuleCatalogController> logger)
    {
        _moduleCatalogService = moduleCatalogService;
        _logger = logger;
    }

    // §8.5 — latência e estado degradado do catálogo, correlacionáveis por TraceIdentifier
    // (mesmo campo já usado em ProblemDetails.correlationId e AuditService.CorrelationId).
    // Erro não tratado ainda cai no GlobalExceptionHandler (loga e responde 500) — o catch
    // aqui só acrescenta o contexto de latência antes de deixar a exceção seguir.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuleCatalogItemDTO>>> Get()
    {
        var stopwatch = Stopwatch.StartNew();
        IReadOnlyList<ModuleCatalogItemDTO> modules;
        try
        {
            modules = await _moduleCatalogService.GetForCurrentUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Falha ao montar catálogo de módulos após {ElapsedMs}ms, correlação {CorrelationId}",
                stopwatch.ElapsedMilliseconds, HttpContext.TraceIdentifier);
            throw;
        }

        var unavailableCount = modules.Count(m => m.OperationalState != OperationalState.AVAILABLE);
        _logger.LogInformation(
            "Catálogo de módulos retornado: {Total} módulo(s), {Unavailable} em estado não disponível, {ElapsedMs}ms, correlação {CorrelationId}",
            modules.Count, unavailableCount, stopwatch.ElapsedMilliseconds, HttpContext.TraceIdentifier);

        return Ok(modules);
    }
}
