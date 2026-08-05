using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;


namespace SeniorCareManager.WebAPI.Data.Builders
{
    public class CarrierBuilder
    {
        public static void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Carrier>().HasKey(pg => pg.Id);
            modelBuilder.Entity<Carrier>().Property(pg => pg.CorporateName)
                .IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Carrier>().Property(pg => pg.TradeName)
                .IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Carrier>().Property(pg => pg.CpfCnpj)
                .IsRequired().HasMaxLength(14);
            modelBuilder.Entity<Carrier>().Property(pg => pg.Street)
                .IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Carrier>().Property(pg => pg.Number)
                .IsRequired().HasMaxLength(5);
            modelBuilder.Entity<Carrier>().Property(pg => pg.District)
                .IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Carrier>().Property(pg => pg.AddressComplement)
                .IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Carrier>().Property(pg => pg.City)
                .IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Carrier>().Property(pg => pg.State)
                .IsRequired().HasMaxLength(2);
            modelBuilder.Entity<Carrier>().Property(pg => pg.PostalCode)
                .IsRequired().HasMaxLength(8);
            modelBuilder.Entity<Carrier>().Property(pg => pg.Phone)
                .IsRequired().HasMaxLength(11);
            modelBuilder.Entity<Carrier>().Property(pg => pg.Email)
                .IsRequired().HasMaxLength(50);

            modelBuilder.Entity<Carrier>().HasData(new List<Carrier>
            {
                new Carrier(1, "Transportes ABC LTDA", "ABC Transportes", "12345678000190",
                "Rua das Flores", "123", "Centro", "Próximo ao banco",
                "São Paulo", "SP", "01001000", "11987654321", "contato@abctransportes.com"),

                new Carrier(2, "Expresso XYZ S/A", "Expresso XYZ", "98765432000180",
                "Avenida Paulista", "456", "Bela Vista", "Esquina com a Rua Augusta",
                "São Paulo", "SP", "01311000", "11976543210", "expresso@xyz.com.br"),

                new Carrier(3, "Translogística EFG ME", "EFG Transportes", "22334455000122",
                "Avenida Rio Branco", "789", "Centro", "Próximo ao metrô",
                "Rio de Janeiro", "RJ", "20040001", "21987654321", "contato@efgtrans.com.br"),
            });
        }
    }
}
