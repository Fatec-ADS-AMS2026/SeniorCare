using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data.Interfaces;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
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

    public async Task<IEnumerable<T>> Get(bool includeInactive = false)
    {
        if (includeInactive)
            return await _dbSet.ToListAsync();

        return await _dbSet.Where(e => EF.Property<bool>(e, "IsActive")).ToListAsync();
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
        // Snapshot do estado atual ANTES de aplicar a mudança — Update nunca lê o valor
        // anterior por conta própria (só recebe a entidade já modificada pelo chamador); sem
        // isso o BeforeValue da auditoria ficaria sempre igual ao AfterValue.
        var entityId = _context.Entry(entity).Property("Id").CurrentValue;
        var beforeSnapshot = await GetSnapshotAsync(entityId);

        AttachAsModified(entity, entityId);
        var entry = _context.Entry(entity);

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

    // §9.2: nunca exclui fisicamente — Remove/Activate só alternam IsActive. Nome "Remove"
    // mantido por compatibilidade com todo call site existente (controllers chamam
    // Remove(id) esperando o verbo HTTP DELETE de sempre).
    public async Task Remove(T entity) => await ToggleActiveAsync(entity, isActive: false, action: "Deactivate");

    public async Task Activate(T entity) => await ToggleActiveAsync(entity, isActive: true, action: "Activate");

    private async Task ToggleActiveAsync(T entity, bool isActive, string action)
    {
        var entityId = _context.Entry(entity).Property("Id").CurrentValue;
        var beforeSnapshot = await GetSnapshotAsync(entityId);

        // Diferente de Update: quem chama Remove/Activate (GenericService, via GetById) já
        // passa a MESMA instância que o EF rastreia desde a consulta — com o xmin real
        // capturado na leitura. Desanexar e reanexar (como Update faz, pra trocar de
        // instância) perderia essa shadow property (não existe em memória fora do
        // ChangeTracker) e faria SaveChanges comparar contra um xmin em branco, gerando um
        // 409 de concorrência falso em toda inativação/reativação. Só anexa se realmente
        // vier destacada (uso direto do repositório, fora do fluxo GenericService normal).
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }
        entry.Property("IsActive").CurrentValue = isActive;

        await SaveChanges();
        await RecordCatalogAuditAsync(action, beforeSnapshot, entity);
    }

    private async Task<T?> GetSnapshotAsync(object? entityId) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => Equals(EF.Property<object>(e, "Id"), entityId));

    // Desanexa qualquer instância já rastreada com o mesmo Id (a entidade recebida como
    // parâmetro não veio de uma query, então nunca está no ChangeTracker) e anexa a
    // recebida como 'Modified'.
    private void AttachAsModified(T entity, object? entityId)
    {
        var trackedEntity = _context.ChangeTracker.Entries<T>()
            .FirstOrDefault(e => e.Property("Id").CurrentValue.Equals(entityId));
        if (trackedEntity != null)
        {
            _context.Entry(trackedEntity.Entity).State = EntityState.Detached;
        }

        _context.Entry(entity).State = EntityState.Modified;
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

    // A maioria dos 9 catálogos é dado de referência puro (sem credencial/segredo) — serializa
    // a entidade inteira. Carrier/Manufacturer/Supplier são exceção: têm CPF/CNPJ, e-mail,
    // telefone e endereço de um terceiro (fornecedor/transportadora), então usam um snapshot
    // reduzido — auditar o dado pessoal inteiro numa tabela agora imutável (§8.6) seria
    // retenção indevida, sem forma de corrigir/expurgar depois (achado da revisão do PR).
    private async Task RecordCatalogAuditAsync(string action, object? beforeValue, object? afterValue)
    {
        await _auditService.RecordAsync(
            AuditEventCategory.CATALOG,
            typeof(T).Name,
            action,
            AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId,
            institutionId: await _currentUserContext.GetInstitutionIdAsync(),
            beforeValue: RedactForAudit(beforeValue),
            afterValue: RedactForAudit(afterValue));
    }

    private static object? RedactForAudit(object? entity) => entity switch
    {
        Carrier c => new { c.Id, c.CorporateName, c.TradeName, c.IsActive },
        Manufacturer m => new { m.Id, m.CorporateName, m.TradeName, m.IsActive },
        Supplier s => new { s.Id, s.CorporateName, s.TradeName, s.IsActive },
        _ => entity,
    };
}