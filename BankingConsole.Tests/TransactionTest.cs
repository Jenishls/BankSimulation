// using BankingConsole.Models;
// using BankingConsole.Models.Enums;

// namespace BankingConsole.Tests.Models;

// public class TransactionTests
// {
//     private static Account CreateAccount(decimal balance = 100)
//     {
//         var account = Account.Create("ACC-01", balance);
//         return account;
//     }

//     [Fact]
//     public void Post_WhenTransactionIsPending_TransitionsToPosted()
//     {
//         var fromAccount = CreateAccount(5000);
//         var toAccount = CreateAccount(2000);

//         var entries = new List<LedgerEntry>
//         {
//             LedgerEntry.Create(fromAccount, Flow.DEBIT, 200),
//             LedgerEntry.Create(toAccount, Flow.CREDIT, 200)
//         };
//         var transaction = new Transaction(entries,EntryType.DEPOSIT, "Test transaction", Guid.NewGuid());
        
//     }
// }