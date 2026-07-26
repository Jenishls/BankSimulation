using BankingConsole.Models.Enums;
using BankingConsole.Models.CustomerUpdate;

namespace BankingConsole.Models.Customer;

public sealed class IndividualCustomer : Customer
{
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public Nationalities Nationality { get; private set; }
    public Occupations Occupation { get; private set; }
    public EmploymentStatus EmploymentStatus { get; private set; }
    public CustomerRole? Nominee { get; private set; }
    public DateTime? NomineeAddedDate { get; private set; }

    private IndividualCustomer()
    {
    }

    public override void ApplyUpdate(CustomerUpdateData data)
    {
        var details = data.Individual
            ?? throw new ArgumentException(
                "Individual update details are required.",
                nameof(data));

        if (data.Institutional is not null)
            throw new ArgumentException(
                "Institutional details cannot be used for an individual customer.",
                nameof(data));

        if (DateTime.UtcNow.Date.AddYears(-18) < details.DateOfBirth.Date)
            throw new ArgumentOutOfRangeException(
                nameof(details.DateOfBirth),
                "Customer must be at least 18 years old.");

        base.ApplyUpdate(data);

        DateOfBirth = details.DateOfBirth;
        Gender = details.Gender;
        Nationality = details.Nationality;
        Occupation = details.Occupation;
        EmploymentStatus = details.EmploymentStatus;
        Nominee = details.Nominee;
    }

    public static IndividualCustomer Create(
        string name,
        List<Address> address,
        List<Contact> contact,
        List<Identity> identity,
        bool kycStatus,
        DateTime dateOfBirth,
        Gender gender,
        Nationalities nationality,
        Occupations occupation,
        EmploymentStatus employmentStatus,
        string? taxId = null,
        DateTime? taxIssuedDate = null,
        DateTime? kycOn = null,
        DateTime? kycNextOn = null,
        CustomerRole? nominee = null,
        DateTime? nomineeAddedDate = null)
    {
        if (DateTime.UtcNow.Date.AddYears(-18) < dateOfBirth.Date)
            throw new ArgumentOutOfRangeException(
                nameof(dateOfBirth),
                "Customer must be at least 18 years old.");

        if (nominee is not null && nomineeAddedDate is null)
            throw new ArgumentException(
                "Nominee added date is required when a nominee is supplied.",
                nameof(nomineeAddedDate));

        var customer = Populate(
            customer: new IndividualCustomer(),
            name: name,
            customerType: CustomerType.INDIVIDUAL,
            address: address,
            contact: contact,
            identity: identity,
            kycStatus: kycStatus,
            taxId: taxId,
            taxIssuedDate: taxIssuedDate,
            kycOn: kycOn,
            kycNextOn: kycNextOn);

        customer.DateOfBirth = dateOfBirth;
        customer.Gender = gender;
        customer.Nationality = nationality;
        customer.Occupation = occupation;
        customer.EmploymentStatus = employmentStatus;
        customer.Nominee = nominee;
        customer.NomineeAddedDate = nomineeAddedDate;

        return customer;
    }
}
