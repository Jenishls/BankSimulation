namespace BankingConsole.Models.Customer;

public class InstitutionalCustomer : Customer
{
    public required string RegistrationNumber{ get; set; }
    public required DateTime RegisteredDate { get; set; }
    public required DateTime StartDate { get; set; }
    public List<CustomerRole>? Roles { get; set;} 
}