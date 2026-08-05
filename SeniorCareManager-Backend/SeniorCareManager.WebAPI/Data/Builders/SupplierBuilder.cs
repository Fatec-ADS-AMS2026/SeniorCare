using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders
{
    public class SupplierBuilder
    {
        public static void Build(ModelBuilder modelBuilder)
        {
            // Configura a chave primária
            modelBuilder.Entity<Supplier>().HasKey(s => s.Id);

            modelBuilder.Entity<Supplier>().Property(s => s.CorporateName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>().Property(s => s.TradeName)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>().Property(s => s.CpfCnpj)
                .IsRequired()
                .HasMaxLength(14);

            modelBuilder.Entity<Supplier>().Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Supplier>().Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(11);

            modelBuilder.Entity<Supplier>().Property(s => s.PostalCode)
                .IsRequired()
                .HasMaxLength(8);

            modelBuilder.Entity<Supplier>().Property(s => s.Street)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>().Property(s => s.Number)
                .IsRequired()
                .HasMaxLength(5);

            modelBuilder.Entity<Supplier>().Property(s => s.District)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Supplier>().Property(s => s.AddressComplement)
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>().Property(s => s.City)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Supplier>().Property(s => s.State)
                .IsRequired()
                .HasMaxLength(2);
        }
    }
}
