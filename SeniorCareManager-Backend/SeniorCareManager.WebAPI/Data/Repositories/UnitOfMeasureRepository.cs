using SeniorCareManager.WebAPI.Services.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Data.Interfaces;
namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class UnitOfMeasureRepository : GenericRepository<UnitOfMeasure>, IUnitOfMeasureRepository
    {
        private readonly AppDbContext _context;

        public UnitOfMeasureRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }
    }
}
