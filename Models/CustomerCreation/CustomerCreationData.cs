using BankingConsole.Models.Enums;

namespace BankingConsole.Models.CustomerCreation;

public sealed class CustomerCreationData
{
    public required string Name { get; init; }
    public required CustomerType CustomerType { get; init; }
    public required bool KycStatus {get; set;}

    public List<Address> Addresses { get; init; } = [];
    public List<Contact> Contacts { get; init; } = [];
    public List<Identity> Identities { get; init; } = [];

    public IndividualCustomerData? Individual { get; init; }
    public InstitutionalCustomerData? Institutional { get; init; }
}