using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Customer;

public class Customer
{
    public Guid CustomerId { get; set; }
    public required string Name { get; set; }
    public required CustomerType CustomerType {get; set;}
    public required List<Address> Address{ get; set; }
    public required List<Contact> Contact{ get; set; }
    public required List<Identity> Identity {get; set;}
    public string? TaxId {get; set;}
    public DateTime? TaxIssuedDate {get; set;}
    public required bool KycStatus {get; set;}
    public DateTime? KycOn {get; set;}
    public DateTime? KycNextOn {get; set;}
    public Customer(){}
}