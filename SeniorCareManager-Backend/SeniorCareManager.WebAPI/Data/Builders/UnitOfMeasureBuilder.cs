using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders
{
    public class UnitOfMeasureBuilder
    {

            public static void Build(ModelBuilder modelBuilder)
            {
                // Configura a chave primária
                modelBuilder.Entity<UnitOfMeasure>().HasKey(pg => pg.Id);
                modelBuilder.Entity<UnitOfMeasure>().Property(pg => pg.Abbreviation).IsRequired().HasMaxLength(3);
                modelBuilder.Entity<UnitOfMeasure>().Property(pg => pg.Description).IsRequired().HasMaxLength(30);


                // Inserção de dados iniciais (opcional)
                modelBuilder.Entity<UnitOfMeasure>()
                    .HasData(new List<UnitOfMeasure>
                    {
                        new UnitOfMeasure(1, "kg", "Kilogram"),
                        new UnitOfMeasure(2, "m", "Meter"),
                        new UnitOfMeasure(3, "l", "Liter"),
                    });
            }
        }
    
}
