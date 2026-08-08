using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class AccountTokenBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountToken>().HasKey(t => t.Id);
        modelBuilder.Entity<AccountToken>().Property(t => t.TokenHash).IsRequired().HasMaxLength(88); // base64(SHA-256)
        modelBuilder.Entity<AccountToken>().HasIndex(t => t.TokenHash).IsUnique();
        modelBuilder.Entity<AccountToken>().HasIndex(t => new { t.UserId, t.Purpose });
    }
}
