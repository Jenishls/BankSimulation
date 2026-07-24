using BankingConsole.Models.Enums;

namespace BankingConsole.Models.CustomerUpdate;

public sealed class CustomerUpdateData
{
    public required Guid CustomerId {get;set;}
    public required string Name { get; init; }
    public required CustomerType CustomerType { get; init; }
    public required bool KycStatus {get; set;}

    public IReadOnlyList<Address> Addresses { get; init; } = [];
    public IReadOnlyList<Contact> Contacts { get; init; } = [];
    public IReadOnlyList<Identity> Identities { get; init; } = [];

    public IndividualCustomerUpdateData? Individual { get; init; }
    public InstitutionalCustomerUpdateData? Institutional { get; init; }
}