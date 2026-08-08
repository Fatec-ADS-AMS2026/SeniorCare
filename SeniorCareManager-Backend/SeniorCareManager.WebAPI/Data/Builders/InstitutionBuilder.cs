using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Data.Builders;

public class InstitutionBuilder
{
    public static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Institution>().HasKey(i => i.Id);
        modelBuilder.Entity<Institution>().Property(i => i.Name).IsRequired().HasMaxLength(200);
    }
}
