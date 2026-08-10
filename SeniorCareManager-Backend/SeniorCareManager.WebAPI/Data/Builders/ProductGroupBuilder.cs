using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ProductGroupBuilder
{   
    public static void Build(ModelBuilder modelBuilder)
    {
        // Configura a chave primária
        modelBuilder.Entity<ProductGroup>().HasKey(pg => pg.Id);
        modelBuilder.Entity<ProductGroup>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<ProductGroup>().Property(pg => pg.Name)
            .IsRequired()
            .HasMaxLength(50);
        modelBuilder.Entity<ProductGroup>().Property(pg => pg.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<ProductGroup>().HasIndex(pg => pg.Name).IsUnique();

        // Inserção de dados iniciais (opcional)
        modelBuilder.Entity<ProductGroup>()
            .HasData(new List<ProductGroup>
            {
                new ProductGroup(1, "Medicamentos") { IsActive = true },
                new ProductGroup(2, "Equipamentos Médicos") { IsActive = true },
                new ProductGroup(3, "Suplementos") { IsActive = true },
            });
    }
}