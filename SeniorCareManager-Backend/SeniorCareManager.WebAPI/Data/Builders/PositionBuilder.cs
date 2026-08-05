using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders
{
    public class PositionBuilder
    {


        public static void Build(ModelBuilder modelBuilder)
        {
            // Configura a chave primária
            modelBuilder.Entity<Position>().HasKey(pg => pg.Id);
            modelBuilder.Entity<Position>().Property(pg => pg.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Inserção de dados iniciais (opcional)
            modelBuilder.Entity<Position>()
                .HasData(new List<Position>
                {
                new Position(1, "Enfermeiros"),
                new Position(2, "Cuidadores"),
                new Position(3, "Cozinheiro"),
                new Position(4, "Administrador"),
                new Position(5, "Nutricionista"),


                });

        }
    }
}
