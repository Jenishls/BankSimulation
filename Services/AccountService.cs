using BankingConsole.Models;
using BankingConsole.Common;
using BankingConsole.Models.Enums;
using BankingConsole.Repository;
using BankingConsole.DB;

namespace BankingConsole.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(AccountRepository accountRepository, UnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult> CreateAccount(Customer customer)
    {
        var accountId = Guid.NewGuid();
        var account = new Account
        {
            AccountId = accountId,
            AccountNumber = AccountNumberGeneratorService.GenerateAccountNumber(
                accountId,
                AccountType.SAVINGS
                ),
            CustomerId = customer.CustomerId,
            Type = AccountType.SAVINGS,
            State = AccountState.ACTIVE
        };

        _accountRepository.AddAccount(account);
        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(
            200,
            true,
            $"Account {account.AccountNumber} for {customer.Name} created successfully.",
            []);
    }

}