namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;

public class FindEmployerAccountExpiredFundsResponse
{
    public decimal TwentyFourthMonthExpiryAmount { get; set; }
    public decimal TwelveMonthExpiryAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime TransactionDate { get; set; }
}