using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models;

[Table("manufacturer")]
public class Manufacturer
{
    [Column("id")]
    public int Id { get; set; }
    [Column("corporate_name")]
    public string CorporateName { get; set; }
    [Column("tradename")]
    public string TradeName { get; set; }
    [Column("cpf_cnpj")]
    public string CpfCnpj { get; set; }
    [Column("phone")]
    public string Phone { get; set; }
    [Column("email")]
    public string Email { get; set; }

    public Manufacturer(int id, string corporateName, string tradeName, string cpfCnpj, string phone, string email)
    {
        Id = id;
        CorporateName = corporateName;
        TradeName = tradeName;
        CpfCnpj = cpfCnpj;
        Phone = phone;
        Email = email;
    }
    public Manufacturer() { }
}