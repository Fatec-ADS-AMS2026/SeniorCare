using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SeniorCareManager.WebAPI.Objects.Models;

[Table("productgroup")]
public class ProductGroup
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }

    [JsonIgnore]
    public ICollection<ProductType>? ProductType { get; set; }

    public ProductGroup()
    {
        
    }

    public ProductGroup(int id, string name)
    {
        Id = id;
        Name = name;
    }
}