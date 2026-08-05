using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities
{
    public class CarrierDTO
    {
        public int Id { get; set; }
        public string CorporateName { get; set; }
        public string TradeName { get; set; }
        public string CpfCnpj { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string District { get; set; }
        public string AdressComplement { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
