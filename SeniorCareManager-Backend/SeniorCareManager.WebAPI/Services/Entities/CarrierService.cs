using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class CarrierService : GenericService<Carrier>, ICarrierService
    {
        private readonly ICarrierRepository _carrierRepository;
        public CarrierService(ICarrierRepository repository) : base(repository)
        {
            _carrierRepository = repository;
        }
    }
}
