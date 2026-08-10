using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context, IAuditService auditService, ICurrentUserContext currentUserContext)
        : base(context, auditService, currentUserContext)
    {
    }
}
