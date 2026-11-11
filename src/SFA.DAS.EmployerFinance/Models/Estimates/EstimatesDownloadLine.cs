namespace SFA.DAS.EmployerFinance.Models.Estimates;

public class EstimatesDownloadLine
{
    public DateOnly Month { get; set; }
    public decimal LevyBalanceStartOfMonth { get; set; }
    public decimal LevyIn { get; set; }
    public decimal LearnerCosts { get; set; }
    public decimal CompletionPayments { get; set; }
    public decimal TransferCosts { get; set; }
    public decimal ExpiredLevy { get; set; }
    public decimal LevyBalanceEndOfMonth { get; set; }
    public decimal CoInvestmentCosts { get; set; }
}
