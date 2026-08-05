using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;

[Table("supplier")]
public class Supplier
{
    [Column("id")]
    public int Id { get; set; }

    [Column("corporate_name")]
    public string CorporateName { get; set; } 

    [Column("trade_name")]
    public string TradeName { get; set; } 

    [Column("cpf_cnpj")]
    public string CpfCnpj { get; set; } 

    [Column("email")]
    public string Email { get; set; }

    [Column("phone")]
    public string Phone { get; set; }  

    [Column("postal_code")]
    public string PostalCode { get; set; }  

    [Column("street")]
    public string Street { get; set; }  

    [Column("number")]
    public string Number { get; set; }  

    [Column("district")]
    public string District { get; set; }  
    [Column("address_complement")]
    public string AddressComplement { get; set; }  

    [Column("city")]
    public string City { get; set; } 

    [Column("state")]
    public string State { get; set; }  

    public Supplier() { }

    public Supplier(int id, string corporateName, string tradeName, string cpfCnpj, string email, string phone, string postalCode, string street, string number, string district, string addressComplement, string city, string state)
    {
        Id = id;
        CorporateName = corporateName;
        TradeName = tradeName;
        CpfCnpj = cpfCnpj;
        Email = email;
        Phone = phone;
        PostalCode = postalCode;
        Street = street;
        Number = number;
        District = district;
        AddressComplement = addressComplement;
        City = city;
        State = state;
    }
}
