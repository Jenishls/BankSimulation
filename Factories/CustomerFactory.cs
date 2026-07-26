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
        
         return IndividualCustomer.Create
        (
            data.Name.Trim(),
            data.Addresses,
            data.Contacts,
            data.Identities,
            data.KycStatus, 
            details.DateOfBirth,
            details.Gender,
            details.Nationality,
            details.Occupation,
            details.EmploymentStatus
        );
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

        return InstitutionalCustomer.Create
        (
            
            data.Name.Trim(),
            data.Addresses,
            data.Contacts,
            data.Identities,
            data.KycStatus,

            details.RegistrationNumber.Trim(),
            details.RegisteredDate,
            details.StartDate,
            details.Roles
        ); 
    }
}
