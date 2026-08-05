using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;


namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class UnitOfMeasureService : GenericService<UnitOfMeasure>, IUnitOfMeasureService
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly IMapper _mapper;

        public UnitOfMeasureService(IUnitOfMeasureRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _unitOfMeasureRepository = repository;
            _mapper = mapper;
        }
    }
}
