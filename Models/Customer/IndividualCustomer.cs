using BankingConsole.Models.Enum;
using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Customer;

public class IndividualCustomer : Customer
{
    public DateTime DateOfBirth {get; set;}
    public Gender Gender {get; set;}
    public Nationalities Nationality {get;set;}
    public Occupations Occupation {get;set;}
    public EmploymentStatus EmploymentStatus{get;set;}
    public CustomerRole? Nominee {get; set;}
    public DateTime NomineeAddedDate {get; set;}    
}