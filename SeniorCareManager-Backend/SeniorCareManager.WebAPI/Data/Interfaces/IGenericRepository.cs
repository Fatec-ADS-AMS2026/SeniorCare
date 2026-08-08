namespace SeniorCareManager.WebAPI.Data.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> Get();
    Task<T> GetById(int id);
    Task Add(T entity);

    // expectedVersion é o xmin (token de concorrência otimista do Postgres) que o
    // cliente leu antes de editar — se a linha mudou desde então, o xmin atual não
    // bate e o SaveChanges lança DbUpdateConcurrencyException (mapeada para 409).
    Task Update(T entity, uint? expectedVersion = null);
    Task Remove(T entity);
    Task<bool> SaveChanges();

    // xmin é uma shadow property (não existe como campo C# em T) — só dá pra ler
    // via metadata do EF, daí o repositório expor esse acesso.
    uint? GetVersion(T entity);
}