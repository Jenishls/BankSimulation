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
    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ITransactionActionRepository transactionActionRepository,
        IUnitOfWork unitOfWork
        )
    {
        _transactionRepository = transactionRepository;
        _transactionActionRepository = transactionActionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult> CreateTransactionAsync(List<LedgerEntry> entries, EntryType entryType, string description, Guid idempotencyKey)
    {
        var existingTransaction = await _transactionRepository.GetByIdempotencyKeyAsync(idempotencyKey);
        
        if(existingTransaction is not null)
        {
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

        await _unitOfWork.SaveChangesAsync();

        return new OperationResult(201, true, "Transaction created successfully.",  []);
    }

    public async Task<OperationResult> UpdateTransactionAsync(Guid transactionId, string description)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            return new OperationResult(
                404,
                false,
                "Transaction not found.",
                [ new ErrorResult(404, "Transaction not found.") ]
                );
        }

        if(transaction.State != TransactionState.PENDING)
        {
            return new OperationResult(
                400, 
                false,
                "Only pending transactions can be approved.",
                [ new ErrorResult(400, "Only pending transactions can be approved.")]
                );
        }
        
        foreach(var entry in transaction.Entries)
        {
            var canApply = entry.Flow == Flow.DEBIT
            ? entry.Account.CanWithdraw(entry.Amount)
            : entry.Account.CanDeposit(entry.Amount);

            if (!canApply)
            {
                return new OperationResult(
                    400,
                    false,
                    $"Entry cannot be applied to account {entry.Account.AccountNumber}",
                    [new ErrorResult(400, "Account validation failed")]
                    );
            }
        }

        foreach (var entry in transaction.Entries)
        {
            var account = entry.Account;
            var accountNumber = account.AccountNumber;

            var succeeded = entry.Flow == Flow.DEBIT
            ? account.Withdraw(entry.Amount)
            : account.Deposit(entry.Amount);

            if (!succeeded)
            {
                return new OperationResult(
                    400,
                    false,
                    $"Could not apply entry for account {account.AccountNumber}",
                    [new ErrorResult(409,$"Account operation failed for account {account.AccountNumber}")]
                    );
            }
        }

        var transactionAction = TransactionAction.Create(
            transaction.TransactionId,
            transaction.State,
            TransactionState.POSTED,
            description,
            "User"
        );

        transaction.State = TransactionState.POSTED;
        transaction.Description = description;

        _transactionActionRepository.Add(transactionAction);

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException)
        {
            return new OperationResult(
                409,
                false,
                "The transaction changed while it was being posted.",
                [new ErrorResult(409, "Concurrency conflict")]
                );
        }

        return new OperationResult(200,true, "Transaction updated successfully.", []);
    } 
}