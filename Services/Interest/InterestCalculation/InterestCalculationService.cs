using BankingConsole.DB;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;
using BankingConsole.Models.Product;
using BankingConsole.Repository;
using BankingConsole.Services.Interest.InterestDue;

namespace BankingConsole.Services.Interest.InterestCalculation;

public class InterestService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInterestCalculator _interestCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterestService> _logger;

    public InterestService(
        IAccountRepository accountRepository,
        IProductRepository productRepository,
        IInterestCalculator interestCalculator,
        IUnitOfWork unitOfWork,
        ILogger<InterestService> logger)
    {
        _accountRepository = accountRepository;
        _productRepository = productRepository;
        _interestCalculator = interestCalculator;
        _unitOfWork = unitOfWork;
        _logger = logger; 
    }
    
    public async Task InterestCalculation(
        CustomerAccount account,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            account.ProductId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Product {account.ProductId} was not found.");

        decimal interestCalculated = _interestCalculator.Calculate(new InterestCalculatorData
        {
            Balance = account.GetBalance(),
            Rate = product.InterestRate   
        });

        account.IncreaseInterestAccured(interestCalculated); // + account.InterestAccured;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

