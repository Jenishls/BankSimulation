namespace BankingConsole.Models.Customer;

public sealed class CustomerAction
{
    public Guid CustomerActionId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public string? OldStateJson { get; set; }
    public required string NewStateJson { get; set; }
    public required string PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }

    public CustomerAction(){}

    public static CustomerAction Create(
        Guid customerId,
        Guid idempotencyKey,
        string? oldStateJson,
        string newStateJson,
        string performedBy)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID is required.", nameof(customerId));

        if (idempotencyKey == Guid.Empty)
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        if (string.IsNullOrWhiteSpace(newStateJson))
            throw new ArgumentException("New state is required.", nameof(newStateJson));

        if (string.IsNullOrWhiteSpace(performedBy))
            throw new ArgumentException("PerformedBy is required.", nameof(performedBy));

        return new CustomerAction
        {
            CustomerActionId = Guid.NewGuid(),
            CustomerId = customerId,
            IdempotencyKey = idempotencyKey,
            OldStateJson = oldStateJson,
            NewStateJson = newStateJson,
            PerformedBy = performedBy.Trim(),
            Timestamp = DateTime.UtcNow
        };
    }
}
