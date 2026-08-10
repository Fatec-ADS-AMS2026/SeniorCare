using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ProductTypeBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        // Configura a chave primária
        modelBuilder.Entity<ProductType>().HasKey(pg => pg.Id);
        modelBuilder.Entity<ProductType>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<ProductType>().Property(pg => pg.Name).IsRequired().HasMaxLength(50);
        modelBuilder.Entity<ProductType>().Property(pg => pg.IsActive).HasDefaultValue(true);
        // Nome único dentro do grupo (não globalmente — grupos diferentes podem ter um
        // tipo com o mesmo nome).
        modelBuilder.Entity<ProductType>().HasIndex(pg => new { pg.ProductGroupId, pg.Name }).IsUnique();
        modelBuilder.Entity<ProductType>().HasOne(pg => pg.ProductGroup)
        .WithMany(pg => pg.ProductType)
        .HasForeignKey(pg => pg.ProductGroupId)
        // Sem isso o EF assume Cascade por padrão (FK obrigatória) — excluir um
        // ProductGroup fisicamente arrastaria seus ProductType junto. Como §9.2 troca
        // exclusão por inativação, isso não deveria mais acontecer pela API, mas o
        // Restrict fica como segunda camada de proteção no próprio banco (mesmo
        // racional de defesa em profundidade da §8).
        .OnDelete(DeleteBehavior.Restrict);

        // Inserção de dados iniciais (opcional)
        modelBuilder.Entity<ProductType>()
            .HasData(new List<ProductType>
            {
                new ProductType(1, "Legumes", 3) { IsActive = true },
                new ProductType(2, "Carnes", 3) { IsActive = true },
                new ProductType(3, "Frutas", 3) { IsActive = true },
            });
    }
}