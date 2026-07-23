namespace BankingConsole.Models;

public class Address
{
    public required string Street{get; set;}
    public required string ZipCode {get;set;}
    public required string City {get; set;}
    public required string Municipality{get;set;}
    public required string State {get; set;}
    public required string Country{get;set;}
    public required bool IsPrimary {get; set;}
}    
