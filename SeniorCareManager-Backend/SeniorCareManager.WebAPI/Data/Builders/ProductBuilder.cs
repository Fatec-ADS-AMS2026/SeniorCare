using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class ProductBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.Id);
        modelBuilder.Entity<Product>().Property<uint>("Version").IsRowVersion();
        modelBuilder.Entity<Product>().Property(p => p.Description).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Product>().Property(p => p.GenericName).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<Product>().Property(p => p.MinimumStock).HasColumnType("numeric(14,3)");
        modelBuilder.Entity<Product>().Property(p => p.CurrentStock).HasColumnType("numeric(14,3)");
        modelBuilder.Entity<Product>().Property(p => p.UnitPrice).HasColumnType("numeric(14,2)");
        modelBuilder.Entity<Product>().Property(p => p.AverageCost).HasColumnType("numeric(14,2)");
        modelBuilder.Entity<Product>().Property(p => p.LastPurchasePrice).HasColumnType("numeric(14,2)");
        modelBuilder.Entity<Product>().Property(p => p.StockValue).HasColumnType("numeric(14,2)");
        modelBuilder.Entity<Product>().Property(p => p.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<Product>().HasIndex(p => p.Description);

        // Restrict (não Cascade, o default do EF pra FK obrigatória) — mesmo racional do
        // ProductType→ProductGroup: um ProductType/UnitOfMeasure com Product ativo vinculado
        // não pode ser removido fisicamente por baixo (a API já nunca faz isso, §9.2).
        modelBuilder.Entity<Product>()
            .HasOne(p => p.ProductType)
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(p => p.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
