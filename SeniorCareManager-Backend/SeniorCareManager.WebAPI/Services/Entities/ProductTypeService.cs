using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ProductTypeService : GenericService<ProductType>, IProductTypeService
{
    private readonly IProductTypeRepository _productTypeRepository;
    private readonly IMapper _mapper;
    
    public ProductTypeService(IProductTypeRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _productTypeRepository = repository;
        _mapper = mapper;
    }
    
}