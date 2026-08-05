using AutoMapper;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class GenericService<T> : IGenericService<T> where T : class
{
    private readonly IGenericRepository<T> _repository;
    private readonly IMapper _mapper;
    private IProductGroupRepository repository;
    private IMapper mapper;

    public GenericService(IGenericRepository<T> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public GenericService(IProductGroupRepository repository, IMapper mapper)
    {
        this.repository = repository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        var entities = await _repository.Get();
        return _mapper.Map<IEnumerable<T>>(entities);
    }

    public async Task<T> GetById(int id)
    {
        var entity = await _repository.GetById(id);
        return _mapper.Map<T>(entity);
    }

    public async Task Create(T entity)
    {
        await _repository.Add(entity);
    }

    public async Task Update(T entity, int id)
    {
        var existingEntity = await _repository.GetById(id); // Supondo que sua entidade tenha um campo Id

        if (existingEntity == null)
        {
            throw new KeyNotFoundException($"Entity with id {id} not found.");
        }

        await _repository.Update(entity);
    }

    public async Task Remove(int id)
    {
        var entity = await _repository.GetById(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Entidade com id: {id} não encontrado");
        }

        await _repository.Remove(entity);
    }
}