using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class HealthInsurancePlanService : GenericService<HealthInsurancePlan>, IHealthInsurancePlanService
{
    private readonly IHealthInsurancePlanRepository _healthInsurancePlanRepository;
    private readonly IMapper _mapper;

    public HealthInsurancePlanService(IHealthInsurancePlanRepository repository, IMapper mapper) : base(repository, mapper)
    {
        _healthInsurancePlanRepository = repository;
        _mapper = mapper;
    }
}
