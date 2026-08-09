using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class UserSessionBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSession>().HasKey(s => s.Id);
        modelBuilder.Entity<UserSession>().Property(s => s.UserAgent).HasMaxLength(500);
        modelBuilder.Entity<UserSession>().Property(s => s.IpAddress).HasMaxLength(64);
        modelBuilder.Entity<UserSession>().HasIndex(s => new { s.UserId, s.RevokedAtUtc });
        modelBuilder.Entity<UserSession>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
