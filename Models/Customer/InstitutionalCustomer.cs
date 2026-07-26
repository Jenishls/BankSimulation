using BankingConsole.Models.Enums;
using BankingConsole.Models.CustomerUpdate;

namespace BankingConsole.Models.Customer;

public sealed class InstitutionalCustomer : Customer
{
    public string RegistrationNumber { get; private set; } = null!;
    public DateTime RegisteredDate { get; private set; }
    public DateTime StartDate { get; private set; }
    public List<CustomerRole> Roles { get; private set; } = new();

    private InstitutionalCustomer()
    {
    }

    public override void ApplyUpdate(CustomerUpdateData data)
    {
        var details = data.Institutional
            ?? throw new ArgumentException(
                "Institutional update details are required.",
                nameof(data));

        if (data.Individual is not null)
            throw new ArgumentException(
                "Individual details cannot be used for an institutional customer.",
                nameof(data));

        if (string.IsNullOrWhiteSpace(details.RegistrationNumber))
            throw new ArgumentException(
                "Registration number is required.",
                nameof(data));

        if (details.StartDate < details.RegisteredDate)
            throw new ArgumentOutOfRangeException(
                nameof(details.StartDate),
                "Start date cannot be before the registered date.");

        base.ApplyUpdate(data);

        RegistrationNumber =
            details.RegistrationNumber.Trim().ToUpperInvariant();
        RegisteredDate = details.RegisteredDate;
        StartDate = details.StartDate;
        Roles = details.Roles.ToList();
    }

    public static InstitutionalCustomer Create(
        string name,
        List<Address> address,
        List<Contact> contact,
        List<Identity> identity,
        bool kycStatus,
        string registrationNumber,
        DateTime registeredDate,
        DateTime startDate,
        List<CustomerRole>? roles = null,
        string? taxId = null,
        DateTime? taxIssuedDate = null,
        DateTime? kycOn = null,
        DateTime? kycNextOn = null)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new ArgumentException("Registration number is required.", nameof(registrationNumber));

        if (startDate < registeredDate)
            throw new ArgumentOutOfRangeException(
                nameof(startDate),
                "Start date cannot be before the registered date.");

        var customer = Populate(
            customer: new InstitutionalCustomer(),
            name: name,
            customerType: CustomerType.INSTITUTIONAL,
            address: address,
            contact: contact,
            identity: identity,
            kycStatus: kycStatus,
            taxId: taxId,
            taxIssuedDate: taxIssuedDate,
            kycOn: kycOn,
            kycNextOn: kycNextOn);

        customer.RegistrationNumber = registrationNumber.Trim().ToUpperInvariant();
        customer.RegisteredDate = registeredDate;
        customer.StartDate = startDate;
        customer.Roles = roles ?? new List<CustomerRole>();

        return customer;
    }
}
