using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductGroupService : GenericService<ProductGroup>, IProductGroupService
{
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly AppDbContext _dbContext;

    public ProductGroupService(IProductGroupRepository repository, AppDbContext dbContext) : base(repository)
    {
        _productGroupRepository = repository;
        _dbContext = dbContext;
    }

    // §9.2/9.3: não inativa um grupo que ainda tem tipo de produto ativo referenciando ele
    // — a FK já é Restrict no banco (ProductTypeBuilder), mas aqui devolve um erro de
    // negócio claro (422) em vez de deixar virar uma violação de constraint.
    public override async Task Remove(int id)
    {
        var hasActiveProductType = await _dbContext.ProductTypes
            .AnyAsync(t => t.ProductGroupId == id && t.IsActive);
        if (hasActiveProductType)
            throw new BusinessRuleException("Não é possível inativar um grupo de produto com tipos de produto ativos vinculados a ele.");

        await base.Remove(id);
    }
}
