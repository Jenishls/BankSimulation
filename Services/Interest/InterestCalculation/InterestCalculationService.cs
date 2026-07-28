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
    private readonly AccountRepository _accountRepository;
    private readonly ProductRepository _productRepository;
    private readonly IInterestCalculator _interestCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterestService> _logger;

    public InterestService(
        AccountRepository accountRepository,
        ProductRepository productRepository,
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
    
    public void InterestCalculation(Account account, CancellationToken cancellationToken)
    {
        var product = _productRepository.GetByIdAsync(account.ProductId);
        decimal interestCalculated = _interestCalculator.Calculate(new InterestCalculatorData
        {
            Balance = account.GetBalance(),
            Rate = product.InterestRate   
        });

        account.IncreaseInterestAccured(interestCalculated); // + account.InterestAccured;

        _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

