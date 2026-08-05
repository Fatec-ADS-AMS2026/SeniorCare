using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductGroupService : GenericService<ProductGroup>, IProductGroupService
{
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly IMapper _mapper;
    
    public ProductGroupService(IProductGroupRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _productGroupRepository = repository;
        _mapper = mapper;
    }
    
}