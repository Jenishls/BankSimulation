using BankingConsole.Models;
using BankingConsole.Models.Customer;
using BankingConsole.Common;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Factories;
using BankingConsole.Repository;
using BankingConsole.DB;

namespace BankingConsole.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AccountFactory _accountFactory;

    public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork, AccountFactory accountFactory)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _accountFactory = accountFactory;
    }

    public async Task<OperationResult> CreateAccount(Customer customer)
        => await CreateAccount(customer, AccountType.SAVINGS);

    public async Task<OperationResult> CreateAccount(Customer customer, AccountType accountType)
    {
        var account = _accountFactory.Create(accountType, customer);

        _accountRepository.AddAccount(account);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(
            200,
            true,
            $"Account {account.AccountNumber} for {customer.Name} created successfully.",
            []);
    }

}
