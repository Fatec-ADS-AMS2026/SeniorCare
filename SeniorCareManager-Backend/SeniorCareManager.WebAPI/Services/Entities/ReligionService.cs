using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;


namespace SeniorCareManager.WebAPI.Services.Entities;

public class ReligionService : GenericService<Religion>, IReligionService
{
    private readonly IReligionRepository _religionRepository;
    private readonly IMapper _mapper;

    public ReligionService(IReligionRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _religionRepository = repository;
        _mapper = mapper;

    }
}
