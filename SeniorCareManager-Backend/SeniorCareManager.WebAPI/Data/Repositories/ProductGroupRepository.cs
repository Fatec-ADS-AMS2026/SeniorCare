using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class ProductGroupRepository : GenericRepository<ProductGroup>,  IProductGroupRepository
{
    private readonly AppDbContext _context;
    
    public ProductGroupRepository(AppDbContext context) : base(context)
    {
        this._context = context;
    }
}