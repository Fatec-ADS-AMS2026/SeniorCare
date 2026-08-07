using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductGroupService : GenericService<ProductGroup>, IProductGroupService
{
    private readonly IProductGroupRepository _productGroupRepository;

    public ProductGroupService(IProductGroupRepository repository) : base(repository)
    {
        _productGroupRepository = repository;
    }

}
