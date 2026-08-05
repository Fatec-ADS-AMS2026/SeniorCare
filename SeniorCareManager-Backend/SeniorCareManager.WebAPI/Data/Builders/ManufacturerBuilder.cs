using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders
{
    public class ManufacturerBuilder
    {
        public static void Build(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Manufacturer>().HasKey(m => m.Id);

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.CorporateName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.TradeName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.CpfCnpj)
                .IsRequired()
                .HasMaxLength(14);

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.Phone)
                .HasMaxLength(11);

            modelBuilder.Entity<Manufacturer>()
                .Property(m => m.Email)
                .HasMaxLength(50);

            // Inserção de dados iniciais
            modelBuilder.Entity<Manufacturer>()
                .HasData(new List<Manufacturer>
                {
                    new Manufacturer(1, "Empresa A", "Trade A", "12345678000195", "12345678901", "contato@empresaa.com"),
                    new Manufacturer(2, "Empresa B", "Trade B", "12345678000196", "23456789012", "contato@empresab.com"),
                    new Manufacturer(3, "Empresa C", "Trade C", "12345678000197", "34567890123", "contato@empresac.com")
                });
        }
    }
}
