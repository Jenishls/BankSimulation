using BankingConsole.Models.Account;
using BankingConsole.Models.Interest;

namespace BankingConsole.Services.Interest.InterestCalculation;

public interface IInterestCalculator
{
    decimal Calculate(InterestCalculatorData data);
}