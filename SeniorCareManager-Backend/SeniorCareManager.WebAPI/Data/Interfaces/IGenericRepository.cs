namespace SeniorCareManager.WebAPI.Data.Interfaces;

public interface IGenericRepository<T> where T : class
{
    // includeInactive=false (padrão) traz só as linhas com IsActive=true (§9.1) —
    // GetById ignora esse filtro de propósito, senão nunca daria pra reativar algo.
    Task<IEnumerable<T>> Get(bool includeInactive = false);
    Task<T> GetById(int id);
    Task Add(T entity);

    // expectedVersion é o xmin (token de concorrência otimista do Postgres) que o
    // cliente leu antes de editar — se a linha mudou desde então, o xmin atual não
    // bate e o SaveChanges lança DbUpdateConcurrencyException (mapeada para 409).
    Task Update(T entity, uint? expectedVersion = null);

    // Nunca exclui fisicamente (§9.2) — marca IsActive=false. Nome mantido por
    // compatibilidade com todo call site existente (Controllers chamam Remove(id)
    // esperando o verbo HTTP DELETE de sempre); só a semântica interna mudou.
    Task Remove(T entity);

    // Inverso do Remove — reativa uma linha inativada por engano.
    Task Activate(T entity);

    Task<bool> SaveChanges();

    // xmin é uma shadow property (não existe como campo C# em T) — só dá pra ler
    // via metadata do EF, daí o repositório expor esse acesso.
    uint? GetVersion(T entity);
}