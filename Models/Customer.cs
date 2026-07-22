namespace BankingConsole.Models;

public class Customer
{
    public Guid CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }

    public Customer(){}

    // public Customer(string Name, string Email, string PhoneNumber)
    // {
    //     this.Name = Name;
    //     this.Email = Email;
    //     this.PhoneNumber = PhoneNumber;
    //     CustomerId = Guid.NewGuid();

    //     Console.WriteLine($"Customer created: {Name} with CustomerId: {CustomerId}");
    // }

}