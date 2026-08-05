using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class CarrierRepository : GenericRepository<Carrier>, ICarrierRepository
    {
        private readonly AppDbContext _context;

        public CarrierRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }
    }
}
