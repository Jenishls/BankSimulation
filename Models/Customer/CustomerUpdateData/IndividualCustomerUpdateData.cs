using BankingConsole.Models.Customer;
using BankingConsole.Models.Enum;
using BankingConsole.Models.Enums;

namespace BankingConsole.Models.CustomerUpdate;
public sealed class IndividualCustomerUpdateData
{
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public Nationalities Nationality { get; init; }
    public Occupations Occupation { get; init; }
    public EmploymentStatus EmploymentStatus { get; init; }
    public CustomerRole? Nominee { get; init; }
}