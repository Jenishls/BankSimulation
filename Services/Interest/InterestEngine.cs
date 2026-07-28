using BankingConsole.DB;
using BankingConsole.Models.Enums;
using BankingConsole.Repository;
using BankingConsole.Services.Interest.InterestCalculation;
using BankingConsole.Services.Interest.InterestPosting;
using BankingConsole.Services.Interest.InterestTax;

namespace BankingConsole.Services.Interest;

public sealed class InterestEngine
{
    private readonly IProductRepository _productRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly InterestCalculationService _calculationService;
    private readonly IInterestTaxCalculator _taxCalculator;
    private readonly InterestPostingService _postingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InterestEngine> _logger;

    public InterestEngine(
        IProductRepository productRepository,
        IAccountRepository accountRepository,
        InterestCalculationService calculationService,
        IInterestTaxCalculator taxCalculator,
        InterestPostingService postingService,
        IUnitOfWork unitOfWork,
        ILogger<InterestEngine> logger)
    {
        _productRepository = productRepository;
        _accountRepository = accountRepository;
        _calculationService = calculationService;
        _taxCalculator = taxCalculator;
        _postingService = postingService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InterestEngineResult>> ExecuteAsync(
        DateTime processingDate,
        CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(
            cancellationToken);
        var results = new List<InterestEngineResult>();

        foreach (var product in products)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var accounts = await _accountRepository
                .GetActiveCustomerAccountsByProductIdAsync(
                    product.ProductId,
                    cancellationToken);

            foreach (var account in accounts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dailyInterest =
                    _calculationService.CalculateDailyInterest(
                        account,
                        product,
                        processingDate);

                var accruedInterest = account.InterestAccured;
                
                var taxAmount =
                    product.InterestFlow == Flow.CREDIT
                        ? _taxCalculator.Calculate(
                            accruedInterest,
                            product.TaxRate)
                        : 0;

                var postingResult =
                    await _postingService.PostInterestIfDueAsync(
                        account,
                        processingDate,
                        taxAmount,
                        cancellationToken);

                results.Add(new InterestEngineResult(
                    product.ProductId,
                    account.AccountId,
                    dailyInterest,
                    accruedInterest,
                    taxAmount,
                    postingResult?.TaxTransaction?.TransactionId,
                    postingResult?.TaxTransaction?.State,
                    postingResult?.InterestTransaction.TransactionId,
                    postingResult?.InterestTransaction.State));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Interest engine processed {ProductCount} products and " +
            "{AccountCount} accounts on {ProcessingDate}",
            products.Count,
            results.Count,
            processingDate.Date);

        return results;
    }
}
