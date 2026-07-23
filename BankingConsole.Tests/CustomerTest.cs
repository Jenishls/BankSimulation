using BankingConsole.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Tests;

public class CustomerTests
{
    [Fact]
    public async Task CreateCustomer_ReturnsCustomerWithExpectedDetails()
    {
        await using var context = CreateContext();
        var service = new CustomerService(context);

        var customer = await service.CreateCustomer();

        customer.Should().NotBeNull();
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john@gmail.com");
        customer.PhoneNumber.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCustomer_WhenCalledTwice_ReturnsDifferentCustomers()
    {
        await using var context = CreateContext();
        var service = new CustomerService(context);

        var firstCustomer = await service.CreateCustomer();
        var secondCustomer = await service.CreateCustomer();

        secondCustomer.Should().NotBeSameAs(firstCustomer);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        return new AppDbContext(options);
    }
}
