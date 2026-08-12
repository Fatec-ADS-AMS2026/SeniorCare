using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
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

    public ModuleCatalogController(IModuleCatalogService moduleCatalogService)
    {
        _moduleCatalogService = moduleCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuleCatalogItemDTO>>> Get()
    {
        return Ok(await _moduleCatalogService.GetForCurrentUserAsync());
    }
}
