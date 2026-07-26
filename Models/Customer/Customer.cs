using BankingConsole.Models.Enums;
using BankingConsole.Models.CustomerUpdate;

namespace BankingConsole.Models.Customer;

public class Customer
{
    public Guid CustomerId { get; protected set; }
    public string Name { get; protected set; } = null!;
    public CustomerType CustomerType { get; protected set; }
    public List<Address> Address { get; protected set; } = null!;
    public List<Contact> Contact { get; protected set; } = null!;
    public List<Identity> Identity { get; protected set; } = null!;
    public string? TaxId { get; protected set; }
    public DateTime? TaxIssuedDate { get; protected set; }
    public bool KycStatus { get; protected set; }
    public DateTime? KycOn { get; protected set; }
    public DateTime? KycNextOn { get; protected set; }

    protected Customer()
    {
    }

    public virtual void ApplyUpdate(CustomerUpdateData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.CustomerId != CustomerId)
            throw new ArgumentException(
                "Update data belongs to a different customer.",
                nameof(data));

        if (data.CustomerType != CustomerType)
            throw new ArgumentException(
                "A customer's type cannot be changed.",
                nameof(data));

        if (string.IsNullOrWhiteSpace(data.Name))
            throw new ArgumentException(
                "Customer name is required.",
                nameof(data));

        if (data.Addresses.Count == 0)
            throw new ArgumentException(
                "At least one address is required.",
                nameof(data));

        if (data.Contacts.Count == 0)
            throw new ArgumentException(
                "At least one contact is required.",
                nameof(data));

        if (data.Identities.Count == 0)
            throw new ArgumentException(
                "At least one identity document is required.",
                nameof(data));

        Name = data.Name.Trim();
        KycStatus = data.KycStatus;
        Address = data.Addresses.ToList();
        Contact = data.Contacts.ToList();
        Identity = data.Identities.ToList();
    }

    public static Customer Create(
        string name,
        CustomerType customerType,
        List<Address> address,
        List<Contact> contact,
        List<Identity> identity,
        bool kycStatus,
        string? taxId = null,
        DateTime? taxIssuedDate = null,
        DateTime? kycOn = null,
        DateTime? kycNextOn = null)
        => Populate(
            customer: new Customer(),
            name: name,
            customerType: customerType,
            address: address,
            contact: contact,
            identity: identity,
            kycStatus: kycStatus,
            taxId: taxId,
            taxIssuedDate: taxIssuedDate,
            kycOn: kycOn,
            kycNextOn: kycNextOn);

    protected static T Populate<T>(
        T customer,
        string name,
        CustomerType customerType,
        List<Address> address,
        List<Contact> contact,
        List<Identity> identity,
        bool kycStatus,
        string? taxId,
        DateTime? taxIssuedDate,
        DateTime? kycOn,
        DateTime? kycNextOn) where T : Customer
    {
        Validate(name, address, contact, identity, kycStatus, taxId, taxIssuedDate, kycOn, kycNextOn);

        customer.CustomerId = Guid.NewGuid();
        customer.Name = name.Trim();
        customer.CustomerType = customerType;
        customer.Address = address;
        customer.Contact = contact;
        customer.Identity = identity;
        customer.KycStatus = kycStatus;
        customer.TaxId = taxId?.Trim();
        customer.TaxIssuedDate = taxIssuedDate;
        customer.KycOn = kycOn;
        customer.KycNextOn = kycNextOn;

        return customer;
    }

    protected static void Validate(
        string name,
        List<Address> address,
        List<Contact> contact,
        List<Identity> identity,
        bool kycStatus,
        string? taxId,
        DateTime? taxIssuedDate,
        DateTime? kycOn,
        DateTime? kycNextOn)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (address is null || address.Count == 0)
            throw new ArgumentException("At least one address is required.", nameof(address));

        if (contact is null || contact.Count == 0)
            throw new ArgumentException("At least one contact is required.", nameof(contact));

        if (identity is null || identity.Count == 0)
            throw new ArgumentException("At least one identity document is required.", nameof(identity));

        if (kycStatus && kycOn is null)
            throw new ArgumentException(
                "KYC completion date is required when KYC status is true.",
                nameof(kycOn));

        if (kycOn is not null && kycNextOn is not null && kycNextOn <= kycOn)
            throw new ArgumentOutOfRangeException(
                nameof(kycNextOn),
                "KYC next review date must be after the KYC completion date.");

        if (taxIssuedDate is not null && string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentException(
                "Tax id is required when a tax issued date is supplied.",
                nameof(taxId));
    }
}
