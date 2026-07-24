namespace BankingConsole.Models;

public class Identity
{
    public Guid AddressId {get; set;}
    public Guid CustomerId {get; set;}
    public required string DocumentNumber{get; set;}
    public DateTime IssuedDate {get; set;}
    public DateTime? ExpiryDate {get; set;}
    public required string IssuingAuthority{ get; set;}
    public required string IssuingCountry{ get; set;}
    public bool IsPrimary{get; set;}
}