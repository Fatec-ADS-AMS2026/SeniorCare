using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Services.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Data.Interfaces;
namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context, IAuditService auditService, ICurrentUserContext currentUserContext) : base(context, auditService, currentUserContext)
        {
            this._context = context;
        }
    }
}
