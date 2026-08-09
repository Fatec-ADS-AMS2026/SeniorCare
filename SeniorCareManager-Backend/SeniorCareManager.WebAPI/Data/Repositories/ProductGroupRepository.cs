using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Services.Interfaces;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class ProductGroupRepository : GenericRepository<ProductGroup>,  IProductGroupRepository
{
    private readonly AppDbContext _context;
    
    public ProductGroupRepository(AppDbContext context, IAuditService auditService, ICurrentUserContext currentUserContext) : base(context, auditService, currentUserContext)
    {
        this._context = context;
    }
}