using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class PermissionGroupBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionGroup>().HasKey(g => g.Id);
        modelBuilder.Entity<PermissionGroup>().Property(g => g.Name).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<PermissionGroup>().Property<uint>("Version").IsRowVersion();

        modelBuilder.Entity<PermissionGroupPermission>().HasKey(x => new { x.PermissionGroupId, x.PermissionId });
        modelBuilder.Entity<PermissionGroupPermission>()
            .HasOne<PermissionGroup>()
            .WithMany()
            .HasForeignKey(x => x.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PermissionGroupPermission>()
            .HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
