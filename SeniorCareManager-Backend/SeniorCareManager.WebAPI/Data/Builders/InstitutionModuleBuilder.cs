using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class InstitutionModuleBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InstitutionModule>().HasKey(im => im.Id);
        modelBuilder.Entity<InstitutionModule>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<InstitutionModule>().Property(im => im.OperationalMessage).HasMaxLength(280);
        modelBuilder.Entity<InstitutionModule>().Property(im => im.IsEnabled).HasDefaultValue(false);
        // Sem HasDefaultValue de banco pro enum: DISABLED (3) já é o default da propriedade
        // em C# (InstitutionModule.cs), e um default de banco diferente do default do CLR
        // (AVAILABLE=0) faz o EF confundir "nunca setado" com "setado explicitamente pra
        // AVAILABLE" — um insert explícito de AVAILABLE viraria DISABLED silenciosamente
        // (aviso real do EF ao gerar a migração, corrigido aqui antes de commitar).

        // Um par {instituição, definição} nunca se repete — mesmo padrão de
        // RoleBuilder/ApplicationUserBuilder pra unicidade escopada por instituição.
        modelBuilder.Entity<InstitutionModule>()
            .HasIndex(im => new { im.InstitutionId, im.ModuleDefinitionId })
            .IsUnique();

        // Catálogos não são hard-deletados (mesmo racional de ProductBuilder) — uma
        // ModuleDefinition com InstitutionModule vinculado não pode ser removida por baixo.
        modelBuilder.Entity<InstitutionModule>()
            .HasOne(im => im.ModuleDefinition)
            .WithMany()
            .HasForeignKey(im => im.ModuleDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // introduce-senior-portal §2.2 — primeiro CHECK constraint do repositório; o resto
        // do código usa só HasMaxLength/índice único como constraint de banco. Faixa fixa
        // porque OperationalState é pinado explicitamente (0-3) e persistido como integer
        // nativo, sem HasConversion.
        modelBuilder.Entity<InstitutionModule>()
            .ToTable(t => t.HasCheckConstraint(
                "ck_institutionmodule_operational_state",
                "operational_state BETWEEN 0 AND 3"));
    }
}
