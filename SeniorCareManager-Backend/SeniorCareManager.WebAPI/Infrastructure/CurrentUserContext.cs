using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SeniorCareManager.WebAPI.Data;

namespace SeniorCareManager.WebAPI.Infrastructure;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    private Guid? _institutionId;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public Guid UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(claim, out var userId))
                throw new InvalidOperationException("Nenhuma identidade autenticada no contexto atual.");
            return userId;
        }
    }

    public async Task<Guid> GetInstitutionIdAsync(CancellationToken cancellationToken = default)
    {
        if (_institutionId.HasValue)
            return _institutionId.Value;

        var user = await _dbContext.Users.FindAsync(new object[] { UserId }, cancellationToken)
            ?? throw new InvalidOperationException("Usuário autenticado não encontrado.");

        _institutionId = user.InstitutionId;
        return _institutionId.Value;
    }
}
