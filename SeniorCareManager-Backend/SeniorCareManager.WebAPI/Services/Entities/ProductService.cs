using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductService : GenericService<Product>, IProductService
{
    public ProductService(IProductRepository repository) : base(repository)
    {
    }
}
