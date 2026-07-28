using BankingConsole.Models;

namespace BankingConsole.Services.Interest.InterestPosting;

public sealed record InterestPostingResult(
    Transaction InterestTransaction,
    Transaction? TaxTransaction);
