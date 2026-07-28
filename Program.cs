using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Middleware;
using BankingConsole.Repository;
using BankingConsole.Services;
using BankingConsole.Services.Interest.InterestCalculation;
using BankingConsole.Services.Interest.InterestDue;
using BankingConsole.Services.Interest.InterestPosting;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
);

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlServer(connectionString)
);

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionActionRepository, TransactionActionRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerActionRepository, CustomerActionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSingleton<AccountNumberGeneratorService>();
builder.Services.AddScoped<IAccountFactory, AccountFactory>();
builder.Services.AddScoped<ICustomerFactory, CustomerFactory>();
builder.Services.AddScoped<IProductFactory, ProductFactory>();
builder.Services.AddSingleton<IInterestPostPolicy, LastDayOfFrequencyPolicy>();
builder.Services.AddSingleton<IInterestPostPolicy, MaturityPolicy>();
builder.Services.AddSingleton<IInterestPostPolicy, PostDatePolicy>();
builder.Services.AddSingleton<IInterestPostResolver, InterestPostResolver>();
builder.Services.AddSingleton<IInterestCalculator, SimpleInterestCalculator>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<InterestService>();
builder.Services.AddScoped<InterestPostingService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapGet("/health", () =>
{
    app.Logger.LogInformation("Health endpoint called");
    return Results.Ok(new { Status = "Healthy" });
});


app.Run();
