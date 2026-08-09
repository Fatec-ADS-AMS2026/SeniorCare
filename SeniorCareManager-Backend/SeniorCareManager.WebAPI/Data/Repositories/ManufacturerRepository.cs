using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Services.Interfaces;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class ManufacturerRepository : GenericRepository<Manufacturer>, IManufacturerRepository
    {
        private readonly AppDbContext _context;

        public ManufacturerRepository(AppDbContext context, IAuditService auditService, ICurrentUserContext currentUserContext) : base(context, auditService, currentUserContext)
        {
            _context = context; 
        }
    }
}
