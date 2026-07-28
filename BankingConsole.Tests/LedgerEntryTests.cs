using BankingConsole.Models;
using BankingConsole.Models.Enums;
using AccountModel = BankingConsole.Models.Account.Account;

namespace BankingConsole.Tests;

public class LedgerEntryTests
{
    [Fact]
    public void Create_WithValidValues_CreatesEntry()
    {
        var account = new TestAccount();

        var entry = LedgerEntry.Create(account, Flow.DEBIT, 125.50m, Currency.EUR);

        Assert.NotEqual(Guid.Empty, entry.LedgerEntryId);
        Assert.Same(account, entry.Account);
        Assert.Equal(Flow.DEBIT, entry.Flow);
        Assert.Equal(125.50m, entry.Amount);
        Assert.Equal(Currency.EUR, entry.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveAmount_Throws(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => LedgerEntry.Create(new TestAccount(), Flow.CREDIT, amount, Currency.EUR));
    }

    [Fact]
    public void Create_WithNullAccount_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => LedgerEntry.Create(null!, Flow.CREDIT, 10m, Currency.EUR));
    }

    private sealed class TestAccount : AccountModel
    {
        public TestAccount()
            : base(
                "TEST-001",
                "Test Account",
                "BER",
                0m,
                AccountState.ACTIVE,
                DateTime.UtcNow,
                AccountType.CUSTOMER)
        {
        }
    }
}
