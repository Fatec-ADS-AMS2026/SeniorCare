using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data.Interfaces;

namespace SeniorCareManager.WebAPI.Data.Repositories;

public class GenericRepository<T>: IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        this._context = context;
        this._dbSet = _context.Set<T>();
    }
    
    public async Task<IEnumerable<T>> Get()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> GetById(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChanges();
    }

    public async Task Update(T entity, uint? expectedVersion = null)
    {
        // Recupera a chave primária (supondo que seja 'Id')
        var entityId = _context.Entry(entity).Property("Id").CurrentValue;

        // Verifica se a entidade com o mesmo Id já está sendo rastreada
        var trackedEntity = _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Property("Id").CurrentValue.Equals(entityId));

        // Se a entidade já estiver sendo rastreada, desanexa
        if (trackedEntity != null)
        {
            _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
        }

        // Anexa a nova entidade e marca como 'Modified'
        var entry = _context.Entry(entity);
        entry.State = EntityState.Modified;

        // "Version" é uma shadow property (uint, IsRowVersion) que o Npgsql mapeia
        // automaticamente para a coluna interna xmin do Postgres — não existe como
        // propriedade em T. Como a entidade acabou de ser anexada (não veio de uma
        // query), o EF assume original = current por padrão, o que tornaria o
        // WHERE xmin = ... inútil. Definir o OriginalValue explicitamente com o que
        // o cliente leu é o que faz o SaveChanges comparar contra o estado atual da
        // linha no banco de fato.
        if (expectedVersion.HasValue && entry.Metadata.FindProperty("Version") != null)
        {
            entry.Property("Version").OriginalValue = expectedVersion.Value;
        }

        // Salva as alterações no banco de dados
        await SaveChanges();
    }

    public async Task Remove(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChanges();
    }

    public async Task<bool> SaveChanges()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public uint? GetVersion(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.Metadata.FindProperty("Version") == null)
        {
            return null;
        }

        return (uint?)entry.Property("Version").CurrentValue;
    }
}