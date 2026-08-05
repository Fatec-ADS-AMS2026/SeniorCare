using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ProductTypeBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        // Configura a chave primária
        modelBuilder.Entity<ProductType>().HasKey(pg => pg.Id);
        modelBuilder.Entity<ProductType>().Property(pg => pg.Name).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ProductType>().HasOne(pg => pg.ProductGroup)
        .WithMany(pg => pg.ProductType)
        .HasForeignKey(pg => pg.ProductGroupId);
        
        // Inserção de dados iniciais (opcional)
        modelBuilder.Entity<ProductType>()
            .HasData(new List<ProductType>
            {
                new ProductType(1, "Legumes", 3),
                new ProductType(2, "Carnes", 3),
                new ProductType(3, "Frutas", 3),
            });
    }
}