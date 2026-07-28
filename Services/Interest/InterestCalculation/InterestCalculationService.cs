using BankingConsole.Common;
using BankingConsole.Models.Account;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Interest;
using BankingConsole.Models.Product;

namespace BankingConsole.Services.Interest.InterestCalculation;

public sealed class InterestCalculationService
{
    private readonly IInterestCalculator _interestCalculator;
    private readonly ILogger<InterestCalculationService> _logger;

    public InterestCalculationService(
        IInterestCalculator interestCalculator,
        ILogger<InterestCalculationService> logger)
    {
        _interestCalculator = interestCalculator;
        _logger = logger;
    }

    public decimal CalculateDailyInterest(
        CustomerAccount account,
        Product product,
        DateTime calculationDate)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(product);

        if (account.ProductId != product.ProductId)
        {
            throw new ValidationException(
                "The account does not belong to the supplied product.");
        }

        if (account.InterestCalculatedOn?.Date >=
            calculationDate.Date)
        {
            return 0;
        }

        var interestBalance = product.ProductType == ProductType.LOAN
            ? account.OutstandingPrincipal.GetValueOrDefault()
            : account.GetBalance();

        var calculatedInterest = _interestCalculator.Calculate(
            new InterestCalculatorData
            {
                Balance = interestBalance,
                Rate = product.InterestRate
            });

        var accrued = account.AccrueDailyInterest(
            calculatedInterest,
            calculationDate);

        if (!accrued)
            return 0;

        _logger.LogInformation(
            "Calculated {InterestAmount} daily interest for " +
            "account {AccountId} on {CalculationDate}",
            calculatedInterest,
            account.AccountId,
            calculationDate.Date);

        return calculatedInterest;
    }
}
