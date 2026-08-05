using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class HealthInsurancePlanRepository : GenericRepository<HealthInsurancePlan>, IHealthInsurancePlanRepository
{
    private readonly AppDbContext _context;

    public HealthInsurancePlanRepository(AppDbContext context) : base(context)
    {
        this._context = context;
    }
}
