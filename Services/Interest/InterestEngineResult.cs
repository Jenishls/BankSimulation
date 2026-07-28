using BankingConsole.Models.Enums;

namespace BankingConsole.Services.Interest;

public sealed record InterestEngineResult(
    Guid ProductId,
    Guid AccountId,
    decimal DailyInterest,
    decimal AccruedInterestBeforePosting,
    decimal TaxAmount,
    Guid? TaxTransactionId,
    TransactionState? TaxTransactionState,
    Guid? InterestTransactionId,
    TransactionState? InterestTransactionState);
