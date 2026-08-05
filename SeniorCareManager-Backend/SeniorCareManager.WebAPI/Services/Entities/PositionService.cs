

using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Data.Repositories;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class PositionService : GenericService<Position>, IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IMapper _mapper;

        public PositionService(IPositionRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _positionRepository = repository;
            _mapper = mapper;
        }

    }
}
