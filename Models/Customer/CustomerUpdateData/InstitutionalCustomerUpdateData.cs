using BankingConsole.Models.Customer;

namespace BankingConsole.Models.CustomerUpdate;

public sealed class InstitutionalCustomerUpdateData
{
    public required string RegistrationNumber { get; init; }
    public DateTime RegisteredDate { get; init; }
    public DateTime StartDate { get; init; }
    public List<CustomerRole> Roles { get; init; } = [];
}