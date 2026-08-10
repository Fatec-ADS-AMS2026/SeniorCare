namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IGenericService<T> where T : class
{
    Task<IEnumerable<T>> GetAll(bool includeInactive = false);
    Task<T> GetById(int id);
    Task Create(T entity);
    Task Update(T entity, int id, uint? expectedVersion = null);

    // Nunca exclui fisicamente (§9.2) — marca IsActive=false.
    Task Remove(int id);

    // Inverso do Remove — reativa uma linha inativada por engano.
    Task Activate(int id);

    uint? GetVersion(T entity);
}