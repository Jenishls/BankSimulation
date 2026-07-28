using BankingConsole.DB;
using BankingConsole.Models;
using BankingConsole.Models.Account;
using BankingConsole.Models.Interest;
using BankingConsole.Models.Product;
using BankingConsole.Repository;

namespace BankingConsole.Services.Interest.InterestDue;

public sealed class InterestPostResolver : IInterestPostResolver
{
    private readonly ProductRepository _productRepository;
    private readonly AccountRepository _accountRepository;
    private readonly IEnumerable<IInterestPostPolicy> _policies;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionActionRepository _transactionActionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public InterestPostResolver(
        IEnumerable<IInterestPostPolicy> policies,
        ProductRepository productRepository,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ITransactionActionRepository transactionActionRepository,
        IUnitOfWork unitOfWork,
        ILogger<TransactionService> logger
        )
    {
        _policies = policies;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
        _transactionActionRepository = transactionActionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public void Resolve(Account account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account); 
        var product = _productRepository.GetByIdAsync(account.ProductId);
        var TodayDate = DateTime.UtcNow;

        var maturityDate = account is TermAccount termAccount ? termAccount.MaturityDate : TodayDate.AddDays(1);
        var postDate = product is LoanProduct loanProduct ? loanProduct.InterestPostDate : TodayDate.AddDays(1);

        var data = new IsDueResolverData
        {
            TodayDate = DateTime.UtcNow,
            Frequency = product.InterestPostingFrequency,
            MaturityDate = maturityDate,
            PostDate = postDate,
            LastInterestPostedOn = account.InterestPostedOn
        };

        foreach(var policy in product.InterestPostPolicies)
        {
            var matchingPolicy = _policies.FirstOrDefault(p => p.Policy == policy);
            if (matchingPolicy == null)
            {
                throw new ArgumentOutOfRangeException("Implementation for the policy not found");    
            }
            if(matchingPolicy.IsDue(data) && account.InterestPostedOn != TodayDate)
            {
                var transactionService = new TransactionService(
                    _transactionRepository,
                    _accountRepository,
                    _transactionActionRepository,
                    _unitOfWork,
                    _logger
                    );
                
                // var glAccount = OfficeAccount.Create()
                // var leg1 = LedgerEntry.Create(account, product.InterestFlow, account.InterestAccured, product.Currency);
                // var leg2 = LedgerEntry.Create(account, product.InterestFlow, account.InterestAccured, product.Currency);
                //Transaction initiate
            }
            
        }




    }

}
