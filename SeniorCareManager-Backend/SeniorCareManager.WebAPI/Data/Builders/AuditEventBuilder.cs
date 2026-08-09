using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class AuditEventBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditEvent>().HasKey(e => e.Id);
        modelBuilder.Entity<AuditEvent>().Property(e => e.Resource).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(e => e.Action).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(e => e.Feature).HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(e => e.TargetScopeKey).HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(e => e.DeterminingLayer).HasMaxLength(50);
        modelBuilder.Entity<AuditEvent>().Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AuditEvent>().Property(e => e.BeforeValue).HasColumnType("jsonb");
        modelBuilder.Entity<AuditEvent>().Property(e => e.AfterValue).HasColumnType("jsonb");

        // Sem navegação/FK para ApplicationUser/Institution de propósito — o log precisa
        // continuar válido independente do ciclo de vida de quem ele referencia (mesmo que,
        // hoje, contas nunca sejam excluídas fisicamente — só mudam de AccountState, §6).
        modelBuilder.Entity<AuditEvent>().HasIndex(e => new { e.InstitutionId, e.OccurredAtUtc });
        modelBuilder.Entity<AuditEvent>().HasIndex(e => e.CorrelationId);
    }
}
