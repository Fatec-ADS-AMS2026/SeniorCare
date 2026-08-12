using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Infrastructure.Validation;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// introduce-senior-portal §3.3 — administração do catálogo institucional. Só
// consulta/altera InstitutionModule (habilitação, ordem, estado, mensagem): não há
// POST nem DELETE porque as linhas são provisionadas de forma idempotente (§2.3), nunca
// criadas/excluídas por este controller (design.md decisão 4-5).
[ApiController]
[Route("api/v1/[controller]")]
public class AdminInstitutionModuleController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminInstitutionModuleController(AppDbContext dbContext, ICurrentUserContext currentUserContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    [HttpGet]
    [RequirePermission("InstitutionModule", "read")]
    public async Task<ActionResult<IReadOnlyList<InstitutionModuleAdminDTO>>> Get()
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var modules = await _dbContext.InstitutionModules
            .Where(im => im.InstitutionId == institutionId)
            .Include(im => im.ModuleDefinition)
            .OrderBy(im => im.Order)
            .ToListAsync();

        return Ok(modules.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    [RequirePermission("InstitutionModule", "read")]
    public async Task<ActionResult<InstitutionModuleAdminDTO>> GetById(int id)
    {
        var institutionModule = await GetInInstitutionAsync(id);
        return Ok(ToDto(institutionModule));
    }

    private InstitutionModuleAdminDTO ToDto(InstitutionModule institutionModule) => new()
    {
        Id = institutionModule.Id,
        ModuleKey = institutionModule.ModuleDefinition!.Key,
        ModuleName = institutionModule.ModuleDefinition!.Name,
        IsEnabled = institutionModule.IsEnabled,
        Order = institutionModule.Order,
        OperationalState = institutionModule.OperationalState,
        OperationalMessage = institutionModule.OperationalMessage,
        CreatedAtUtc = institutionModule.CreatedAtUtc,
        UpdatedAtUtc = institutionModule.UpdatedAtUtc,
        RowVersion = _dbContext.Entry(institutionModule).Property<uint>("Version").CurrentValue,
    };

    [HttpPut("{id}")]
    [RequirePermission("InstitutionModule", "write")]
    public async Task<ActionResult<InstitutionModuleAdminDTO>> Put(int id, InstitutionModuleUpdateRequest request)
    {
        var errors = OperationalMessageSanitizer.Validate(request.OperationalMessage);
        if (errors.Count > 0)
            throw new BusinessRuleException(string.Join(" ", errors));

        var institutionModule = await GetInInstitutionAsync(id);
        var beforeSnapshot = new
        {
            institutionModule.IsEnabled,
            institutionModule.Order,
            institutionModule.OperationalState,
            institutionModule.OperationalMessage,
        };

        institutionModule.IsEnabled = request.IsEnabled;
        institutionModule.Order = request.Order;
        institutionModule.OperationalState = request.OperationalState;
        institutionModule.OperationalMessage = request.OperationalMessage;
        institutionModule.UpdatedAtUtc = DateTime.UtcNow;

        // §3.4 — concorrência otimista: a entidade veio de uma query nesta mesma
        // requisição (rastreada pelo EF com o xmin real lido do banco), mas sobrescrever
        // o OriginalValue com o RowVersion que o cliente enviou é o que garante que o
        // SaveChanges compare contra a versão que o cliente efetivamente leu (não contra
        // uma releitura implícita) — mesmo mecanismo de GenericRepository.Update.
        _dbContext.Entry(institutionModule).Property<uint>("Version").OriginalValue = request.RowVersion;

        await _dbContext.SaveChangesAsync();

        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        await _auditService.RecordAsync(AuditEventCategory.CATALOG, "InstitutionModule", "Update", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: institutionId,
            targetScopeType: AccessScopeType.INSTITUTION, targetScopeKey: institutionModule.ModuleDefinition!.Key,
            beforeValue: beforeSnapshot,
            afterValue: new { institutionModule.IsEnabled, institutionModule.Order, institutionModule.OperationalState, institutionModule.OperationalMessage });

        return Ok(ToDto(institutionModule));
    }

    private async Task<InstitutionModule> GetInInstitutionAsync(int id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var institutionModule = await _dbContext.InstitutionModules
            .Include(im => im.ModuleDefinition)
            .SingleOrDefaultAsync(im => im.Id == id && im.InstitutionId == institutionId);

        if (institutionModule == null) throw new KeyNotFoundException("Módulo não encontrado.");
        return institutionModule;
    }
}
