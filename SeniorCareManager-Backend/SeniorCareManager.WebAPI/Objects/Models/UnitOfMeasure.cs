
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;
[Table("unitofmeasure")]

public class UnitOfMeasure
{
    [Column("id")]
    public int Id { get; set; }


    [Column("description")]
    public string Description { get; set; }


    [Column("abbreviation")]
    public string Abbreviation { get; set; }


    public UnitOfMeasure()
    {

    }

    public UnitOfMeasure(int id, string abbreviation, string description)
    {
        Id = id;
        Description = description;
        Abbreviation = abbreviation;
    }
}
