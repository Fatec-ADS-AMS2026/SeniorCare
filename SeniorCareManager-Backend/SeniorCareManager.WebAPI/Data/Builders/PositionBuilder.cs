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
            modelBuilder.Entity<Position>().Property<uint>("Version").IsRowVersion();
            modelBuilder.Entity<Position>().Property(pg => pg.Name)
                .IsRequired()
                .HasMaxLength(50);
            modelBuilder.Entity<Position>().Property(pg => pg.IsActive).HasDefaultValue(true);
            modelBuilder.Entity<Position>().HasIndex(pg => pg.Name).IsUnique();

            // Inserção de dados iniciais (opcional)
            modelBuilder.Entity<Position>()
                .HasData(new List<Position>
                {
                new Position(1, "Enfermeiros") { IsActive = true },
                new Position(2, "Cuidadores") { IsActive = true },
                new Position(3, "Cozinheiro") { IsActive = true },
                new Position(4, "Administrador") { IsActive = true },
                new Position(5, "Nutricionista") { IsActive = true },


                });

        }
    }
}
