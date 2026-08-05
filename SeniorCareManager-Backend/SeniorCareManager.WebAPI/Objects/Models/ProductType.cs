using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SeniorCareManager.WebAPI.Objects.Models;

[Table("producttype")]
public class ProductType
{
    [Column("id")]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; }
    [Column("productgroupid")][ForeignKey("productgroup")]
    public int ProductGroupId { get; set; }

    [JsonIgnore]
    public ProductGroup? ProductGroup { get; set; }
    public ProductType(){ }

    public ProductType(int id, string name, int productGroupId){
        Id = id;
        Name = name;
        ProductGroupId = productGroupId;
    }
}
