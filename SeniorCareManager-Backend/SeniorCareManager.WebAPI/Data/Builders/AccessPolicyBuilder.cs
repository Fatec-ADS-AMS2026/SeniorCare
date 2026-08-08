using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class AccessPolicyBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessPolicy>().HasKey(p => p.Id);
        modelBuilder.Entity<AccessPolicy>().Property(p => p.Resource).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<AccessPolicy>().Property(p => p.Action).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<AccessPolicy>().Property(p => p.Feature).HasMaxLength(100);
        modelBuilder.Entity<AccessPolicy>().Property(p => p.ScopeKey).HasMaxLength(100);
        modelBuilder.Entity<AccessPolicy>()
            .HasIndex(p => new { p.PolicyKey, p.Version }).IsUnique();
        modelBuilder.Entity<AccessPolicy>()
            .HasIndex(p => new { p.InstitutionId, p.Resource, p.Action, p.Feature, p.State });
    }
}
