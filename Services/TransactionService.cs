using Azure;
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
        IAccountRepository accountRepository,
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

    public async Task<OperationResult> CreateTransactionAsync(List<LedgerEntry> entries, EntryType entryType, string description, Guid idempotencyKey)
    {
        _logger.LogInformation(
            "Transaction initiated. IdempotencyKey: {IdempotencyKey}, EntryCount: {EntryCount}, EntryType : {EntryType}, Description : {Description} ",
            idempotencyKey,
            entries.Count,
            entryType,
            description
        );
       
        var existingTransaction = await _transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey);
        
        if(existingTransaction is not null)
        {
            _logger.LogWarning(
                "Duplicate transaction. IdempotencyKey : {IdempotencyKey}, Existing Transaction Id: {TransactionId}",
                idempotencyKey,
                existingTransaction.TransactionId);

            return new OperationResult
            (
              200,
              true,
              $"Transaction {existingTransaction.TransactionId} already exisits.",
              []  
            );
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
            "User"
        );

        _transactionRepository.Add(transaction);
        _transactionActionRepository.Add(transactionAction);
        
        try{
            await _unitOfWork.SaveChangesAsync();
        }catch(DbUpdateException ex){
            throw new ConflictException($"Database error while creating transaction. IdempotencyKey: {idempotencyKey}, TransactionId : {transaction.TransactionId}. {ex}");
        }
        _logger.LogInformation(
            "Transaction created successfully. IdempotencyKey: {IdempotencyKey}, TransactionId: {TransactionId}",
            idempotencyKey,
            transaction.TransactionId
            );
        return new OperationResult(201, true, "Transaction created successfully.",  []);
    }

    public async Task<OperationResult> UpdateTransactionAsync(Guid transactionId, string description, Guid idempotencyKey)
    {
        _logger.LogInformation(
            "Transaction Update Initiated. IdempotencyKey : {IdempotencyKey}, TransactionId: {TransactionId}",
            idempotencyKey,
            transactionId
        );

        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
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
            "User"
        );

        _transactionActionRepository.Add(transactionAction);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException)
        {
            throw new ConflictException("The transaction changed while it was being posted");
        }
        
        _logger.LogInformation(
            "Transaction updated successfully. IdempotencyKey: {IdempotencyKey}, TransactionId: {TransactionId}",
            idempotencyKey,
            transaction.TransactionId
            );

        return new OperationResult(200,true, "Transaction updated successfully.", []);
    } 
}