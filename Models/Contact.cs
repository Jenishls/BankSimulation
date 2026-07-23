namespace BankingConsole.Models;

public class Contact
{
    public required string CountryCode { get; set;}
    public string? Phone { get; set;}
    public required string Mobile { get; set;}
    public required string Email { get; set;}
    public bool IsPrimary { get; set;}
}