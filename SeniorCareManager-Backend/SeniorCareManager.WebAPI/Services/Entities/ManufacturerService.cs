using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ManufacturerService : GenericService<Manufacturer>,IManufacturerService
{
    private readonly IManufacturerRepository _manufacturerRepository;

    public ManufacturerService(IManufacturerRepository repository) : base(repository)
    {
        _manufacturerRepository = repository;
    }

}
