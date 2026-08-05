using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;

[Table("position")]
    public class Position
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }

    public Position(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

