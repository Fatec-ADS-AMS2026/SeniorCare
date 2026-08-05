using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class HealthInsurancePlanBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        // Configura a chave primária
        modelBuilder.Entity<HealthInsurancePlan>().HasKey(hip => hip.Id);
        modelBuilder.Entity<HealthInsurancePlan>().Property(hip => hip.Name).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<HealthInsurancePlan>().Property(hip => hip.Type).IsRequired().HasMaxLength(1);
        modelBuilder.Entity<HealthInsurancePlan>().Property(hip => hip.Abbreviation).IsRequired().HasMaxLength(5);

        // Inserção de dados iniciais
        modelBuilder.Entity<HealthInsurancePlan>()
            .HasData(new List<HealthInsurancePlan>
            {
                // Inserir dados iniciais
                new HealthInsurancePlan(1, "Unimed", HealthPlanType.PRIVATE, "UNI"),
                new HealthInsurancePlan(2, "Hapvida", HealthPlanType.PRIVATE, "HAP"),
                new HealthInsurancePlan(3, "Sistema Único de Saúde", HealthPlanType.PUBLIC, "SUS"),
            });
    }
}

