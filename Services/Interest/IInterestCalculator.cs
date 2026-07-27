using BankingConsole.Models.Account;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest;

public interface IInterestCalculator
{
    decimal InterestCalculator(InterestCalculatorData data);
}