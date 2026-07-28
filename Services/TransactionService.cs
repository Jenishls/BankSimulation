using BankingConsole.Common;
using BankingConsole.DB;
using BankingConsole.Models;
using BankingConsole.Models.Enums;
using BankingConsole.Repository;
using Microsoft.EntityFrameworkCore;

namespace BankingConsole.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionActionRepository _transactionActionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;
    public TransactionService(
        ITransactionRepository transactionRepository,
        ITransactionActionRepository transactionActionRepository,
        IUnitOfWork unitOfWork,
        ILogger<TransactionService> logger
        )
    {
        _transactionRepository = transactionRepository;
        _transactionActionRepository = transactionActionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Transaction> CreateTransactionAsync(
        List<LedgerEntry> entries,
        EntryType entryType,
        string description,
        Guid idempotencyKey,
        CancellationToken cancellationToken = default,
        string performedBy = "System")
    {
        ArgumentNullException.ThrowIfNull(entries);

        _logger.LogInformation(
            "Transaction initiated. IdempotencyKey: {IdempotencyKey}, EntryCount: {EntryCount}, EntryType : {EntryType}, Description : {Description} ",
            idempotencyKey,
            entries.Count,
            entryType,
            description
        );
       
        var existingTransaction = await _transactionRepository
            .GetByIdempotencyKeyAsync(
                idempotencyKey,
                cancellationToken);
        
        if(existingTransaction is not null)
        {
            _logger.LogWarning(
                "Duplicate transaction. IdempotencyKey : {IdempotencyKey}, Existing Transaction Id: {TransactionId}",
                idempotencyKey,
                existingTransaction.TransactionId);

            return existingTransaction;
        }

        var transaction = Transaction.Create(
            entries,
            entryType,
            description,
            idempotencyKey);

        var transactionAction = TransactionAction.Create(
            transaction.TransactionId,
            null,
            TransactionState.PENDING,
            description,
            performedBy
        );

        _transactionRepository.Add(transaction);
        _transactionActionRepository.Add(transactionAction);
        
        try{
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }catch(DbUpdateException){
            throw new ConflictException(
                $"Database error while creating transaction. " +
                $"IdempotencyKey: {idempotencyKey}, " +
                $"TransactionId: {transaction.TransactionId}.");
        }
        _logger.LogInformation(
            "Transaction created successfully. IdempotencyKey: {IdempotencyKey}, TransactionId: {TransactionId}",
            idempotencyKey,
            transaction.TransactionId
            );
        return transaction;
    }

    public async Task<Transaction> PostTransactionAsync(
        Guid transactionId,
        string description,
        CancellationToken cancellationToken = default,
        string performedBy = "System")
    {
        _logger.LogInformation(
            "Transaction posting initiated. TransactionId: {TransactionId}",
            transactionId
        );

        var transaction = await _transactionRepository.GetByIdAsync(
            transactionId,
            cancellationToken);
        if (transaction == null)
        {
            throw new NotFoundException($"Transaction {transactionId} not found");
        }
        var previousState = transaction.State;
        transaction.Post(description);

        var transactionAction = TransactionAction.Create(
            transaction.TransactionId,
            previousState,
            transaction.State,
            description,
            performedBy
        );

        _transactionActionRepository.Add(transactionAction);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch(DbUpdateConcurrencyException)
        {
            throw new ConflictException("The transaction changed while it was being posted");
        }
        
        _logger.LogInformation(
            "Transaction posted successfully. TransactionId: {TransactionId}",
            transaction.TransactionId
            );

        return transaction;
    } 
}
