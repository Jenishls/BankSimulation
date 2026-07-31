using BankingConsole.Common;
using BankingConsole.Models.Customer;
using BankingConsole.Models.CustomerCreation;
using BankingConsole.Models.CustomerUpdate;
using BankingConsole.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankingConsole.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomerController : ControllerBase
{
    private const string IdempotencyHeader = "Idempotency-Key";
    private readonly CustomerService _customerService;

    public CustomerController(CustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Customer>>> GetAll(
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetCustomersAsync(
            cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<Customer>> GetById(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByIdAsync(
            customerId,
            cancellationToken);

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(
        [FromBody] CustomerCreationData creationData,
        [FromHeader(Name = IdempotencyHeader)] Guid? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var key = ResolveIdempotencyKey(idempotencyKey);
        var customer = await _customerService.CreateCustomerAsync(
            key,
            creationData,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { customerId = customer.CustomerId },
            customer);
    }

    [HttpPut("{customerId:guid}")]
    public async Task<ActionResult<Customer>> Update(
        Guid customerId,
        [FromBody] CustomerUpdateData updateData,
        [FromHeader(Name = IdempotencyHeader)] Guid? idempotencyKey,
        [FromHeader(Name = "X-Performed-By")] string? performedBy,
        CancellationToken cancellationToken)
    {
        if (customerId != updateData.CustomerId)
        {
            throw new ValidationException(
                "The route customer id must match the request customer id.");
        }

        var key = ResolveIdempotencyKey(idempotencyKey);
        var customer = await _customerService.UpdateCustomerAsync(
            key,
            updateData,
            string.IsNullOrWhiteSpace(performedBy) ? "API" : performedBy,
            cancellationToken);

        return Ok(customer);
    }

    [HttpDelete("{customerId:guid}")]
    public async Task<IActionResult> Delete(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        await _customerService.DeleteCustomerAsync(
            customerId,
            cancellationToken);

        return NoContent();
    }

    private Guid ResolveIdempotencyKey(Guid? idempotencyKey)
    {
        if (idempotencyKey == Guid.Empty)
        {
            throw new ValidationException(
                $"{IdempotencyHeader} must be a non-empty GUID.");
        }

        var key = idempotencyKey ?? Guid.NewGuid();
        Response.Headers[IdempotencyHeader] = key.ToString();
        return key;
    }
}
