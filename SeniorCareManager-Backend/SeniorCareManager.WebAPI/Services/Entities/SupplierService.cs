using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;
using System.Threading;

namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class SupplierService : GenericService<Supplier>, ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;
        public SupplierService(ISupplierRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _supplierRepository = repository;
            _mapper = mapper;
        }
    }
}