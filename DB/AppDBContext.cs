using BankingConsole.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<TransactionAction> TransactionActions => Set<TransactionAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomer(modelBuilder);
        ConfigureAccount(modelBuilder);
        ConfigureTransaction(modelBuilder);
        ConfigureLedgerEntry(modelBuilder);
        ConfigureTransactionAction(modelBuilder);
    }

    private static void ConfigureCustomer(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();

        customer.HasKey(c => c.CustomerId);

        customer.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        customer.Property(c => c.Email)
            .HasMaxLength(320)
            .IsRequired();

        customer.Property(c => c.PhoneNumber)
            .HasMaxLength(32);

        customer.HasIndex(c => c.Email)
            .IsUnique();
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        var account = modelBuilder.Entity<Account>();

        account.HasKey(a => a.AccountId);

        account.Property(a => a.AccountNumber)
            .HasMaxLength(32)
            .IsRequired();

        account.Property(a => a.Balance)
            .HasPrecision(18, 2);

        account.Property(a => a.RowVersion)
            .IsRowVersion();

        account.HasIndex(a => a.AccountNumber)
            .IsUnique();

        account.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        var transaction = modelBuilder.Entity<Transaction>();

        transaction.HasKey(t => t.TransactionId);

        transaction.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired();

        transaction.Property(t => t.Amount)
            .HasPrecision(18, 2);

        transaction.Property(t => t.RowVersion)
            .IsRowVersion();

        transaction.HasIndex(t => t.IdempotencyKey)
            .IsUnique();

        transaction.HasMany(t => t.Entries)
            .WithOne()
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureLedgerEntry(ModelBuilder modelBuilder)
    {
        var entry = modelBuilder.Entity<LedgerEntry>();

        entry.HasKey(e => e.LedgerEntryId);

        entry.Property(e => e.Amount)
            .HasPrecision(18, 2);

        entry.HasOne(e => e.Account)
            .WithMany()
            .HasForeignKey("AccountId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureTransactionAction(ModelBuilder modelBuilder)
    {
        var action = modelBuilder.Entity<TransactionAction>();

        action.HasKey(a => a.TransactionActionId);

        action.Property(a => a.Description)
            .HasMaxLength(500)
            .IsRequired();

        action.Property(a => a.PerformedBy)
            .HasMaxLength(200)
            .IsRequired();

        action.HasOne<Transaction>()
            .WithMany()
            .HasForeignKey(a => a.TransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
