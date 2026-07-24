namespace BankingConsole.Models.Customer;

public class CustomerRole
{
    public required string RoleType {get;set;}
    public required DateTime EffectiveDate {get; set;}
    public DateTime? ExpiryDate {get; set;}
    public required IndividualCustomer IndividualCustomer {get; set;}

}
