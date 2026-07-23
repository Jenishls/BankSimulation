using BankingConsole.Models.Enums;

namespace BankingConsole.Models;

public class Customer
{
    public Guid CustomerId { get; set; }
    public required string Name { get; set; }
    public required Gender Gender{get; set;} 
    public required string Nationality { get; set; }
    public DateTime DateOfBirth { get; set;}
    public required List<Address> Address{ get; set; }
    public required List<Contact> Contact{ get; set; }
    public required List<Identity> Identity {get; set;}
    public Customer(){}
    public static Customer Create(string name, Gender gender, string nationality, DateTime dob, List<Address> addresses, List<Contact> contacts, List<Identity> identities)
    {
        return new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = name.Trim(),
            Gender = gender,
            Nationality = nationality,
            DateOfBirth = dob,
            Address = addresses,
            Contact = contacts,
            Identity = identities
        };
    }

}