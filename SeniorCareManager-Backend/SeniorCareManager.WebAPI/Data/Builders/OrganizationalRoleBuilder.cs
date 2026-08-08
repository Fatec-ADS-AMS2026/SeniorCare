using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class OrganizationalRoleBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationalRole>().HasKey(r => r.Id);
        modelBuilder.Entity<OrganizationalRole>().Property(r => r.Name).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<OrganizationalRole>().HasIndex(r => new { r.InstitutionId, r.Name }).IsUnique();

        modelBuilder.Entity<OrganizationalRolePermissionGroup>().HasKey(x => new { x.OrganizationalRoleId, x.PermissionGroupId });
        modelBuilder.Entity<OrganizationalRolePermissionGroup>()
            .HasOne<OrganizationalRole>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationalRoleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrganizationalRolePermissionGroup>()
            .HasOne<PermissionGroup>()
            .WithMany()
            .HasForeignKey(x => x.PermissionGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
