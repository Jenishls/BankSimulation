using System.Text.Json;
using System.Text.Json.Serialization;
using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Models.Customer;
using BankingConsole.Models.CustomerCreation;
using BankingConsole.Models.CustomerUpdate;
using BankingConsole.Repository;

namespace BankingConsole.Services;

public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerActionRepository _customerActionRepository;
    private readonly ICustomerFactory _customerFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        ICustomerActionRepository customerActionRepository,
        ICustomerFactory customerFactory,
        IUnitOfWork unitOfWork,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _customerActionRepository = customerActionRepository;
        _customerFactory = customerFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Customer> CreateCustomerAsync(
        Guid idempotencyKey,
        CustomerCreationData creationData,
        CancellationToken cancellationToken = default)
    {
        var existingAction = await _customerActionRepository.GetByIdempotencyKeyAsync(idempotencyKey);
        if(existingAction is not null)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(existingAction.CustomerId);
            return existingCustomer!;
        }

        var customer = _customerFactory.Create(creationData);
        var newState = JsonSerializer.Serialize(customer);
        var customerAction = CustomerAction.Create(
            customer.CustomerId,
            idempotencyKey,
            null,
            newState,
            "User"
            );

        _customerRepository.AddCustomer(customer);
        _customerActionRepository.Add(customerAction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created {CustomerType} customer {CustomerId}",
            customer.CustomerType,
            customer.CustomerId);

        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(
        Guid idempotencyKey,
        CustomerUpdateData customerUpdateData,
        string performedBy,
        CancellationToken cancellationToken = default)
    {
        var existingAction =
            await _customerActionRepository
                .GetByIdempotencyKeyAsync(idempotencyKey);

        if (existingAction is not null)
        {
            return await _customerRepository.GetByIdAsync(
                existingAction.CustomerId)
                ?? throw new InvalidOperationException(
                    $"CustomerAction exists for idempotency key " +
                    $"{idempotencyKey}, but customer " +
                    $"{existingAction.CustomerId} was not found.");
        }

        var customer =
            await _customerRepository.GetByIdAsync(
                customerUpdateData.CustomerId)
            ?? throw new NotFoundException(
                $"Customer {customerUpdateData.CustomerId} was not found.");

        if (customer.CustomerType != customerUpdateData.CustomerType)
        {
            throw new ConflictException(
                "A customer's type cannot be changed.");
        }

        var oldStateJson = JsonSerializer.Serialize(customer);

        ApplyUpdates(customer, customerUpdateData);

        var newStateJson = JsonSerializer.Serialize(customer);

        var customerAction = CustomerAction.Create(
            customer.CustomerId,
            idempotencyKey,
            oldStateJson,
            newStateJson,
            performedBy);

        _customerActionRepository.Add(customerAction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated {CustomerType} customer {CustomerId}",
            customer.CustomerType,
            customer.CustomerId);

        return customer;
    }

    private static void ApplyUpdates(
        Customer customer,
        CustomerUpdateData data)
    {
        customer.ApplyUpdate(data);
    }
}
