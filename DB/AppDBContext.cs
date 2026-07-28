using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.Customer;
using BankingConsole.Models.Enums;
using BankingConsole.Models.Product;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<IndividualCustomer> IndividualCustomers => Set<IndividualCustomer>();
    public DbSet<InstitutionalCustomer> InstitutionalCustomers => Set<InstitutionalCustomer>();
    public DbSet<CustomerRole> CustomerRoles => Set<CustomerRole>();
    public DbSet<CustomerAction> CustomerActions => Set<CustomerAction>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<TransactionAction> TransactionActions => Set<TransactionAction>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCustomer(modelBuilder);
        ConfigureCustomerRole(modelBuilder);
        ConfigureCustomerAction(modelBuilder);
        ConfigureProduct(modelBuilder);
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

        customer.Property(c => c.TaxId)
            .HasMaxLength(50);

        customer.HasDiscriminator(c => c.CustomerType)
            .HasValue<Customer>(CustomerType.ALL)
            .HasValue<IndividualCustomer>(CustomerType.INDIVIDUAL)
            .HasValue<InstitutionalCustomer>(CustomerType.INSTITUTIONAL);

        customer.OwnsMany(c => c.Address, address =>
        {
            address.ToTable("CustomerAddresses");
            address.WithOwner().HasForeignKey("CustomerId");
            address.Property<Guid>("AddressId");
            address.HasKey("CustomerId", "AddressId");

            address.Property(a => a.Street).HasMaxLength(200).IsRequired();
            address.Property(a => a.ZipCode).HasMaxLength(20).IsRequired();
            address.Property(a => a.City).HasMaxLength(100).IsRequired();
            address.Property(a => a.Municipality).HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasMaxLength(100).IsRequired();
        });

        customer.OwnsMany(c => c.Contact, contact =>
        {
            contact.ToTable("CustomerContacts");
            contact.WithOwner().HasForeignKey("CustomerId");
            contact.Property<Guid>("ContactId");
            contact.HasKey("CustomerId", "ContactId");

            contact.Property(c => c.CountryCode).HasMaxLength(8).IsRequired();
            contact.Property(c => c.Phone).HasMaxLength(32);
            contact.Property(c => c.Mobile).HasMaxLength(32).IsRequired();
            contact.Property(c => c.Email).HasMaxLength(320).IsRequired();
            contact.HasIndex(c => c.Email);
        });

        customer.OwnsMany(c => c.Identity, identity =>
        {
            identity.ToTable("CustomerIdentities");
            identity.WithOwner().HasForeignKey("CustomerId");
            identity.Property<Guid>("IdentityId");
            identity.HasKey("CustomerId", "IdentityId");

            identity.Property(i => i.DocumentNumber).HasMaxLength(100).IsRequired();
            identity.Property(i => i.IssuingAuthority).HasMaxLength(200).IsRequired();
            identity.Property(i => i.IssuingCountry).HasMaxLength(100).IsRequired();
            identity.HasIndex(i => new { i.DocumentNumber, i.IssuingCountry })
                .IsUnique();
        });

        var individual = modelBuilder.Entity<IndividualCustomer>();
        individual.HasOne(i => i.Nominee)
            .WithOne()
            .HasForeignKey<CustomerRole>("NomineeForCustomerId")
            .OnDelete(DeleteBehavior.SetNull);

        var institution = modelBuilder.Entity<InstitutionalCustomer>();
        institution.Property(i => i.RegistrationNumber)
            .HasMaxLength(100)
            .IsRequired();
        institution.HasIndex(i => i.RegistrationNumber)
            .IsUnique();
        institution.HasMany(i => i.Roles)
            .WithOne()
            .HasForeignKey("InstitutionalCustomerId")
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureCustomerRole(ModelBuilder modelBuilder)
    {
        var role = modelBuilder.Entity<CustomerRole>();

        role.Property<Guid>("CustomerRoleId");
        role.HasKey("CustomerRoleId");

        role.Property(r => r.RoleType)
            .HasMaxLength(100)
            .IsRequired();

        role.HasOne(r => r.IndividualCustomer)
            .WithMany()
            .HasForeignKey("IndividualCustomerId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCustomerAction(ModelBuilder modelBuilder)
    {
        var action = modelBuilder.Entity<CustomerAction>();

        action.HasKey(a => a.CustomerActionId);

        action.Property(a => a.OldStateJson)
            .HasColumnType("nvarchar(max)");

        action.Property(a => a.NewStateJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        action.Property(a => a.PerformedBy)
            .HasMaxLength(200)
            .IsRequired();

        action.Property(a => a.Timestamp)
            .IsRequired();

        action.HasIndex(a => a.IdempotencyKey)
            .IsUnique();

        action.HasIndex(a => new { a.CustomerId, a.Timestamp });

        action.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
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

        account.Property(a => a.InterestAccured)
            .HasPrecision(18, 2);

        account.Property(a => a.RowVersion)
            .IsRowVersion();

        account.HasIndex(a => a.AccountNumber)
            .IsUnique();

        account.HasDiscriminator<string>("AccountType")
            .HasValue<SavingAccount>("SAVING")
            .HasValue<TermAccount>("TERM")
            .HasValue<LoanAccount>("LOAN")
            .HasValue<OfficeAccount>("OFFICE");

        account.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        account.HasOne<Product>()
            .WithMany()
            .HasForeignKey(a => a.ProductId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TermAccount>()
            .Property(a => a.Principal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LoanAccount>()
            .Property(a => a.OriginalPrincipal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<LoanAccount>()
            .Property(a => a.OutstandingPrincipal)
            .HasPrecision(18, 2);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<Product>();

        product.HasKey(p => p.ProductId);

        product.Property(p => p.ProductCode)
            .HasMaxLength(50)
            .IsRequired();

        product.HasIndex(p => p.ProductCode)
            .IsUnique();

        product.Property(p => p.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        product.Property(p => p.MinimumAmount)
            .HasPrecision(18, 2);

        product.Property(p => p.InterestRate)
            .HasPrecision(18, 6);

        product.Property(p => p.TaxPercentage)
            .HasPrecision(5, 2);

        product.Property(p => p.WithdrawalLimitAmount)
            .HasPrecision(18, 2);

        product.PrimitiveCollection(p => p.InterestPostPolicies)
            .HasMaxLength(100)
            .IsRequired();

        product.Ignore(p => p.ProductType);

        product.HasDiscriminator<ProductType>("ProductKind")
            .HasValue<SavingProduct>(ProductType.SAVING)
            .HasValue<TermProduct>(ProductType.TERM)
            .HasValue<LoanProduct>(ProductType.LOAN)
            .HasValue<OfficeProduct>(ProductType.OFFICE);

        product.Property<ProductType>("ProductKind")
            .HasColumnName("ProductType");

        modelBuilder.Entity<LoanProduct>()
            .Property(p => p.PenaltyInterestRate)
            .HasPrecision(18, 6);
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
