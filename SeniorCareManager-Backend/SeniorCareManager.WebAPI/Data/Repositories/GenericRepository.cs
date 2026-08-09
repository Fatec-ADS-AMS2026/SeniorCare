using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Data.Repositories;

// Único ponto de escrita dos 9 catálogos simples (§3b) — por isso é também o único hook
// necessário pra auditar CATALOG (§8), em vez de repetir a chamada em cada controller.
public class GenericRepository<T>: IGenericRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserContext _currentUserContext;

    public GenericRepository(AppDbContext context, IAuditService auditService, ICurrentUserContext currentUserContext)
    {
        this._context = context;
        this._dbSet = _context.Set<T>();
        this._auditService = auditService;
        this._currentUserContext = currentUserContext;
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
        // Depois do SaveChanges — só aí o Id (identity/serial) já foi preenchido pelo banco.
        await RecordCatalogAuditAsync("Create", beforeValue: null, afterValue: entity);
    }

    public async Task Update(T entity, uint? expectedVersion = null)
    {
        // Recupera a chave primária (supondo que seja 'Id')
        var entityId = _context.Entry(entity).Property("Id").CurrentValue;

        // Snapshot do estado atual ANTES de aplicar a mudança — Update nunca lê o valor
        // anterior por conta própria (só recebe a entidade já modificada pelo chamador); sem
        // isso o BeforeValue da auditoria ficaria sempre igual ao AfterValue.
        var beforeSnapshot = await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => Equals(EF.Property<object>(e, "Id"), entityId));

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
        await RecordCatalogAuditAsync("Update", beforeSnapshot, entity);
    }

    public async Task Remove(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChanges();
        await RecordCatalogAuditAsync("Delete", beforeValue: entity, afterValue: null);
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

    // Serializa a entidade inteira como Before/After — seguro aqui porque os 9 catálogos são
    // dado de referência puro (sem credencial, token ou segredo). Revisitar se algum catálogo
    // um dia ganhar um campo sensível.
    private async Task RecordCatalogAuditAsync(string action, object? beforeValue, object? afterValue)
    {
        await _auditService.RecordAsync(
            AuditEventCategory.CATALOG,
            typeof(T).Name,
            action,
            AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId,
            institutionId: await _currentUserContext.GetInstitutionIdAsync(),
            beforeValue: beforeValue,
            afterValue: afterValue);
    }
}