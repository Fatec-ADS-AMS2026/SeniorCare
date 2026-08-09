using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class RoleBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasKey(r => r.Id);
        modelBuilder.Entity<Role>().Property(r => r.Name).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Role>().HasIndex(r => new { r.InstitutionId, r.Name }).IsUnique();
        modelBuilder.Entity<Role>().Property<uint>("Version").IsRowVersion();

        modelBuilder.Entity<RolePermissionGroup>().HasKey(x => new { x.RoleId, x.PermissionGroupId });
        modelBuilder.Entity<RolePermissionGroup>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RolePermissionGroup>()
            .HasOne<PermissionGroup>()
            .WithMany()
            .HasForeignKey(x => x.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>().HasKey(x => x.Id);
        modelBuilder.Entity<UserRole>().HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        modelBuilder.Entity<UserRole>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserRole>()
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
