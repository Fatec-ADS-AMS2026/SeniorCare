using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities
{
    public class UnitOfMeasureService : GenericService<UnitOfMeasure>, IUnitOfMeasureService
    {
        private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;
        private readonly AppDbContext _dbContext;

        public UnitOfMeasureService(IUnitOfMeasureRepository repository, AppDbContext dbContext) : base(repository)
        {
            _unitOfMeasureRepository = repository;
            _dbContext = dbContext;
        }

        // §9.2/9.3: não inativa uma unidade que ainda tem produto ativo referenciando ela.
        public override async Task Remove(int id)
        {
            var hasActiveProduct = await _dbContext.Products.AnyAsync(p => p.UnitOfMeasureId == id && p.IsActive);
            if (hasActiveProduct)
                throw new BusinessRuleException("Não é possível inativar uma unidade de medida com produtos ativos vinculados a ela.");

            await base.Remove(id);
        }
    }
}
