using SeniorCareManager.WebAPI.Services.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Data.Interfaces;
namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }
    }
}
