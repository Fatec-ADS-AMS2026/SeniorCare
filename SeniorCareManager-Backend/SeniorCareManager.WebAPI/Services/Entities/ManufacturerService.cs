using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ManufacturerService : GenericService<Manufacturer>,IManufacturerService
{
    private readonly IManufacturerRepository _manufacturerRepository;
    private readonly IMapper _mapper;

    public ManufacturerService(IManufacturerRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _manufacturerRepository = repository;
        _mapper = mapper;
    }

}