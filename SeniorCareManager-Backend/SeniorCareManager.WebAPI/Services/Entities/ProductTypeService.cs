using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductTypeService : GenericService<ProductType>, IProductTypeService
{
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly AppDbContext _dbContext;

    public ProductTypeService(IProductTypeRepository repository, AppDbContext dbContext) : base(repository)
    {
        _productTypeRepository = repository;
        _dbContext = dbContext;
    }

    // §9.2/9.3: não inativa um tipo que ainda tem produto ativo referenciando ele.
    public override async Task Remove(int id)
    {
        var hasActiveProduct = await _dbContext.Products.AnyAsync(p => p.ProductTypeId == id && p.IsActive);
        if (hasActiveProduct)
            throw new BusinessRuleException("Não é possível inativar um tipo de produto com produtos ativos vinculados a ele.");

        await base.Remove(id);
    }
}
