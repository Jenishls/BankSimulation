using BankingConsole.Models.Enums;
namespace BankingConsole.Models;

public class TransactionAction
{
    public Guid TransactionActionId { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; }
    public TransactionState? PreviousState { get; set; }
    public TransactionState NewState { get; set; }
    public required string Description { get; set; }
    public required string PerformedBy { get; set; }
    public DateTime Timestamp { get; set; }

    public TransactionAction() { }

    public static TransactionAction Create(Guid transactionId, TransactionState? previousState, TransactionState newState, string description, string performedBy)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "Description is required.",
                nameof(description));

        if (string.IsNullOrWhiteSpace(performedBy))
            throw new ArgumentException(
                "PerformedBy is required.",
                nameof(performedBy));

            return new TransactionAction
            {
                TransactionId = transactionId,
                PreviousState = previousState,
                NewState = newState,
                Description = description,
                PerformedBy = performedBy,
                Timestamp = DateTime.UtcNow
            };
    }
}