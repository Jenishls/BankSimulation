using BankingConsole.Models.Account;
using BankingConsole.Models.Product;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Tests;

public sealed class AppDbContextModelTests
{
    [Fact]
    public void Model_MapsProductAndCustomerAccountReferences()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;" +
                "Database=BankingConsoleModelTest;" +
                "Trusted_Connection=True;")
            .Options;

        using var context = new AppDbContext(options);

        var product = context.Model.FindEntityType(typeof(Product));
        var customerAccount =
            context.Model.FindEntityType(typeof(CustomerAccount));

        product.Should().NotBeNull();
        customerAccount.Should().NotBeNull();

        product!.GetForeignKeys()
            .SelectMany(key => key.Properties)
            .Select(property => property.Name)
            .Should()
            .Contain(
                nameof(Product.InterestOfficeAccountId),
                nameof(Product.TaxOfficeAccountId));

        customerAccount!.GetForeignKeys()
            .SelectMany(key => key.Properties)
            .Select(property => property.Name)
            .Should()
            .Contain(
                nameof(CustomerAccount.FundingAccountId),
                nameof(CustomerAccount.MaturitySettlementAccountId),
                nameof(CustomerAccount.DisbursementAccountId),
                nameof(CustomerAccount.RepaymentAccountId));
    }
}
