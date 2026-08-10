using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;



    [Table("religion")]
    public class Religion
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public Religion()
        {

        }

        public Religion(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
