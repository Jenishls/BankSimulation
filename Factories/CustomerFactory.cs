using BankingConsole.Factories;
using BankingConsole.Models.Customer;
using BankingConsole.Models.CustomerCreation;
using BankingConsole.Models.Enums;

namespace BankingConsole.Factories;

public sealed class CustomerFactory : ICustomerFactory
{
    public Customer Create(CustomerCreationData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        ValidateCommonData(data);
        return data.CustomerType switch
        {
            CustomerType.INDIVIDUAL => CreateIndividual(data),
            CustomerType.INSTITUTIONAL => CreateInstitutional(data),
            _ => throw new ArgumentOutOfRangeException(nameof(data.CustomerType), data.CustomerType, "Unsupported customer Type")
        };
    }

    private static void ValidateCommonData(CustomerCreationData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
        {
            throw new ArgumentException("Customer name is required.", nameof(data.Name));
        }
    }

    private static IndividualCustomer CreateIndividual(CustomerCreationData data)
    {
        var details = data.Individual ?? throw new ArgumentException("Individual details are required for an individual customer.");
        if (data.Institutional is not null)
        {
            throw new ArgumentException(
                "Institutional details cannot be provided for an individual customer.");
        }

         return new IndividualCustomer
        {
            CustomerId = Guid.NewGuid(),
            CustomerType = CustomerType.INDIVIDUAL,
            Name = data.Name.Trim(),
            Address = data.Addresses,
            Contact = data.Contacts,
            Identity = data.Identities,
            KycStatus = data.KycStatus, 

            DateOfBirth = details.DateOfBirth,
            Gender = details.Gender,
            Nationality = details.Nationality,
            Occupation = details.Occupation,
            EmploymentStatus = details.EmploymentStatus,
            Nominee = details.Nominee
        };
    }

    private static InstitutionalCustomer CreateInstitutional(CustomerCreationData data)
    {
               var details = data.Institutional
            ?? throw new ArgumentException(
                "Institutional details are required for an institutional customer.");

        if (data.Individual is not null)
        {
            throw new ArgumentException(
                "Individual details cannot be provided for an institutional customer.");
        }

        return new InstitutionalCustomer
        {
            CustomerId = Guid.NewGuid(),
            CustomerType = CustomerType.INSTITUTIONAL,
            Name = data.Name.Trim(),
            Address = data.Addresses,
            Contact = data.Contacts,
            Identity = data.Identities,
            KycStatus = data.KycStatus,

            RegistrationNumber = details.RegistrationNumber.Trim(),
            RegisteredDate = details.RegisteredDate,
            StartDate = details.StartDate,
            Roles = details.Roles,
        }; 
    }
}
