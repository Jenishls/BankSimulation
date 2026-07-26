using BankingConsole.DB;
using BankingConsole.Factories;
using BankingConsole.Middleware;
using BankingConsole.Repository;
using BankingConsole.Services;
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
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerActionRepository, CustomerActionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<AccountFactory>();
builder.Services.AddScoped<ICustomerFactory, CustomerFactory>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<CustomerService>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapGet("/health", () =>
{
    app.Logger.LogInformation("Health endpoint called");
    return Results.Ok(new { Status = "Healthy" });
});


app.Run();
