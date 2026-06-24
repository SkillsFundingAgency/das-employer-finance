namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class ExpiredFundsTransactionDetailsViewModel
{
    public decimal TwentyFourthMonthExpiryAmount { get; set; }
    public decimal TwelveMonthExpiryAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime TransactionDate { get; set; }
    public string HashedAccountId { get; set; }
}