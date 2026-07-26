using BankingConsole.Models.Enums;

namespace BankingConsole.Models.Account;

public sealed class LoanAccount : Account
{
    public decimal OriginalPrincipal { get; private set; }
    public decimal OutstandingPrincipal { get; private set; }
    public Guid DisbursementAccountId { get; private set; }
    public int RepaymentInstallments{get; private set;}
    private LoanAccount()
    {
    }

    private LoanAccount(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        DateTime accountOpenDate,
        decimal originalPrincipal,
        Guid disbursementAccountId,
        int repaymentInstallments
        )
        : base(
            accountNumber,
            customerId,
            productId,
            branchCode,
            balance: 0,
            accountState: AccountState.ACTIVE,
            accountOpenDate: accountOpenDate,
            interestAccrued: null,
            interestPostedOn: null)
    {
        OriginalPrincipal = originalPrincipal;
        OutstandingPrincipal = originalPrincipal;
        DisbursementAccountId = disbursementAccountId;
        RepaymentInstallments = repaymentInstallments;
    }

    public static LoanAccount Create(
        string accountNumber,
        Guid customerId,
        Guid productId,
        string branchCode,
        decimal principal,
        Guid disbursementAccountId,
        int repaymentInstallments)
    {
        if (principal <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(principal),
                "Loan principal must be greater than zero.");

        if (disbursementAccountId == Guid.Empty)
            throw new ArgumentException(
                "Disbursement account id is required.",
                nameof(disbursementAccountId));

        return new LoanAccount(
            accountNumber,
            customerId,
            productId,
            branchCode,
            DateTime.UtcNow,
            principal,
            disbursementAccountId, 
            repaymentInstallments
            );
    }
}
