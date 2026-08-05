using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Data.Repositories
{
    public class PositionRepository : GenericRepository<Position>, IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext context) : base(context)
        {
            this._context = context;
        }


    }
}
