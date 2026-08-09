using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Interceptors;

// §8.6, primeira camada: nenhuma UPDATE/DELETE em AuditEvent passa por SaveChanges, mesmo que
// alguém chame o DbContext diretamente (fora de IAuditService, que só faz Add). A segunda
// camada é uma trigger no próprio Postgres (ver migração) — sobrevive a acesso direto ao banco.
public class AuditImmutabilityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        EnsureNoMutationOfAuditEvents(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        EnsureNoMutationOfAuditEvents(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void EnsureNoMutationOfAuditEvents(DbContext? context)
    {
        if (context == null)
            return;

        var hasMutation = context.ChangeTracker.Entries<AuditEvent>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (hasMutation)
            throw new InvalidOperationException("Eventos de auditoria são append-only — update/delete não é permitido.");
    }
}
