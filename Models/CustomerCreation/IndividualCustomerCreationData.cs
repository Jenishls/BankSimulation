using BankingConsole.Models.Customer;
using BankingConsole.Models.Enums;

namespace BankingConsole.Models.CustomerCreation;
public sealed class IndividualCustomerData
{
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public Nationalities Nationality { get; init; }
    public Occupations Occupation { get; init; }
    public EmploymentStatus EmploymentStatus { get; init; }
    public CustomerRole? Nominee { get; init; }
}