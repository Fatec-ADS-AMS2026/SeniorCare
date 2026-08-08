using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ApplicationUserBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApplicationUser>().Property(u => u.DisplayName).IsRequired().HasMaxLength(200);

        // IdentityUserContext configura NormalizedUserName como único globalmente por
        // padrão; como UserName espelha o e-mail e a identidade é delimitada por
        // instituição (design.md decisão 6), a unicidade correta é composta com
        // InstitutionId — nunca global.
        modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.NormalizedUserName).IsUnique(false);

        // Identidade é individual e vinculada à instituição: mesmo e-mail normalizado não
        // pode existir duas vezes dentro da mesma instituição.
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => new { u.InstitutionId, u.NormalizedEmail })
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => new { u.InstitutionId, u.NormalizedUserName })
            .IsUnique();
    }
}
