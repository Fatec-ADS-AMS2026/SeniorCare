using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Data.Repositories;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class PositionService : GenericService<Position>, IPositionService
    {
        private readonly IPositionRepository _positionRepository;

        public PositionService(IPositionRepository repository) : base(repository)
        {
            _positionRepository = repository;
        }

    }
}
