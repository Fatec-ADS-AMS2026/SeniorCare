using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class ReligionService : GenericService<Religion>, IReligionService
{
    private readonly IReligionRepository _religionRepository;

    public ReligionService(IReligionRepository repository) : base(repository)
    {
        _religionRepository = repository;

    }
}
