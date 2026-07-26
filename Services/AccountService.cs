using BankingConsole.Models.Customer;
using BankingConsole.Common;
using BankingConsole.Models.Enums;
using BankingConsole.Factories;
using BankingConsole.Repository;
using BankingConsole.DB;
using BankingConsole.Models.AccountCreation;
using BankingConsole.Models.Account;

namespace BankingConsole.Services;

public class AccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AccountFactory _accountFactory;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        AccountFactory accountFactory,
        ILogger<AccountService> logger
        )
    {
        _accountRepository = accountRepository;
        _accountFactory = accountFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> CreateAccount(AccountCreationData data)
    {
        _logger.LogInformation(
            "Account creation initiated"
        );
        var account = _accountFactory.Create(data);

        _accountRepository.AddAccount(account);

        await _unitOfWork.SaveChangesAsync();

        return account;
    }

}
