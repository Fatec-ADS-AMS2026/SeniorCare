using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;



    [Table("religion")]
    public class Religion
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }

        public Religion()
        {

        }

        public Religion(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
