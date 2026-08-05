using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class ReligionRepository : GenericRepository<Religion>, IReligionRepository
{
    private readonly AppDbContext _context;

    public ReligionRepository(AppDbContext context) : base(context)
    {
        this._context = context;
    }
}