using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly IGenericRepository<T> _repository;

    public GenericService(IGenericRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<T>> GetAll(bool includeInactive = false)
    {
        return await _repository.Get(includeInactive);
    }

    public async Task<T> GetById(int id)
    {
        return await _repository.GetById(id);
    }

    public async Task Create(T entity)
    {
        await _repository.Add(entity);
    }

    public async Task Update(T entity, int id, uint? expectedVersion = null)
    {
        var existingEntity = await _repository.GetById(id); // Supondo que sua entidade tenha um campo Id

        if (existingEntity == null)
        {
            throw new KeyNotFoundException($"Entity with id {id} not found.");
        }

        await _repository.Update(entity, expectedVersion);
    }

    // virtual: catálogos referenciados por outra entidade (ProductGroup, ProductType,
    // UnitOfMeasure — §9.2/9.3) sobrescrevem pra checar dependente ativo antes de inativar.
    public virtual async Task Remove(int id)
    {
        var entity = await _repository.GetById(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entidade com id: {id} não encontrado");
        }

        await _repository.Remove(entity);
    }

    public async Task Activate(int id)
    {
        var entity = await _repository.GetById(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entidade com id: {id} não encontrado");
        }

        await _repository.Activate(entity);
    }

    public uint? GetVersion(T entity) => _repository.GetVersion(entity);
}
