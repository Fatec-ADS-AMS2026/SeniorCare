using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class OrganizationalRoleAssignmentBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrganizationalRoleAssignment>().HasKey(a => a.Id);
        modelBuilder.Entity<OrganizationalRoleAssignment>().Property(a => a.ScopeKey).HasMaxLength(100);
        modelBuilder.Entity<OrganizationalRoleAssignment>()
            .HasIndex(a => new { a.UserId, a.OrganizationalRoleId, a.ValidFrom });
        modelBuilder.Entity<OrganizationalRoleAssignment>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrganizationalRoleAssignment>()
            .HasOne<OrganizationalRole>()
            .WithMany()
            .HasForeignKey(a => a.OrganizationalRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
