using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class UserPermissionOverrideBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermissionOverride>().HasKey(o => o.Id);
        modelBuilder.Entity<UserPermissionOverride>().Property(o => o.Resource).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<UserPermissionOverride>().Property(o => o.Action).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<UserPermissionOverride>().Property(o => o.Feature).HasMaxLength(100);
        modelBuilder.Entity<UserPermissionOverride>().Property(o => o.ScopeKey).HasMaxLength(100);
        modelBuilder.Entity<UserPermissionOverride>().Property(o => o.Justification).IsRequired().HasMaxLength(1000);
        modelBuilder.Entity<UserPermissionOverride>()
            .HasIndex(o => new { o.UserId, o.Resource, o.Action, o.Feature });
        modelBuilder.Entity<UserPermissionOverride>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
