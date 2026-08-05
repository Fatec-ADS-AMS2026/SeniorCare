using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;


public class ReligionBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        // Configura a chave primária
        modelBuilder.Entity<Religion>().HasKey(pg => pg.Id);
        modelBuilder.Entity<Religion>().Property(pg => pg.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Inserção de dados iniciais (opcional)
        modelBuilder.Entity<Religion>()
            .HasData(new List<Religion>
            {
                new Religion(1, "Católico"),
                new Religion(2, "Evangelico"),
                new Religion(3, "Ateu"),
            });
    }
}
