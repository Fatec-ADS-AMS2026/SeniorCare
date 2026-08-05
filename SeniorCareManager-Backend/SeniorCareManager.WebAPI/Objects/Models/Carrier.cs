using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("carrier")]
    public class Carrier
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("corporatename")]
        public string CorporateName { get; set; }

        [Column("tradename")]
        public string TradeName { get; set; }

        [Column("cpfcnpj")]
        public string CpfCnpj { get; set; }

        [Column("street")]
        public string Street { get; set; }

        [Column("number")]
        public string Number { get; set; }

        [Column("district")]
        public string District { get; set; }

        [Column("addresscomplement")]
        public string AddressComplement { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("state")]
        public string State { get; set; }

        [Column("postalcode")]
        public string PostalCode { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("email")]
        public string Email { get; set; }

        public Carrier() { }

        public Carrier(int id, string corporateName, string tradeName, string cpfCnpj, string street, string number, string district, string adressComplement, string city, string state, string postalCode, string phone, string email)
        {
            Id = id;
            CorporateName = corporateName;
            TradeName = tradeName;
            CpfCnpj = cpfCnpj;
            Street = street;
            Number = number;
            District = district;
            AddressComplement = adressComplement;
            City = city;
            State = state;
            PostalCode = postalCode;
            Phone = phone;
            Email = email;
        }
    }
}
