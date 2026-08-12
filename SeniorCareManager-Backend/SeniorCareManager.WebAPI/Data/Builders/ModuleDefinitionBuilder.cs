using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ModuleDefinitionBuilder
{
    // Mesmos GUIDs fixos das permissões Module/care e Module/stock seedadas em
    // PermissionBuilder — precisam bater exatamente, já que HasData grava a FK direto.
    private static readonly Guid CarePermissionId = Guid.Parse("E08D83BF-9FF8-4575-BBF1-3E62F7054048");
    private static readonly Guid StockPermissionId = Guid.Parse("F628285F-A074-4053-A1C3-22526C55AA93");

    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ModuleDefinition>().HasKey(md => md.Id);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.Key).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.Description).IsRequired().HasMaxLength(300);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.Icon).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.Path).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<ModuleDefinition>().Property(md => md.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<ModuleDefinition>().HasIndex(md => md.Key).IsUnique();

        modelBuilder.Entity<ModuleDefinition>()
            .HasOne(md => md.RequiredPermission)
            .WithMany()
            .HasForeignKey(md => md.RequiredPermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // introduce-senior-portal §2.3 — só as 2 definições aprovadas nesta mudança
        // (design.md decisão 1: assistência e estoque). Novos módulos exigem migração
        // nova (validados por ModuleDefinitionValidator), não uma API de criação livre.
        modelBuilder.Entity<ModuleDefinition>()
            .HasData(new List<ModuleDefinition>
            {
                new ModuleDefinition(
                    1, "care", "Assistência", "Cuidado e acompanhamento dos residentes",
                    "HeartStraight", "/care", CarePermissionId),
                new ModuleDefinition(
                    2, "stock", "Estoque", "Controle de insumos e produtos",
                    "Package", "/stock", StockPermissionId),
            });
    }
}
