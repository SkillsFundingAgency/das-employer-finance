namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class FinanceDashboardViewModel
{
    public string HashedAccountId { get; set; }
    public decimal CurrentLevyFunds { get; set; }
    public decimal TotalSpendForLastYear { get; set; }
    public decimal AvailableFunds { get; set; }
    public decimal FundingExpected { get; set; }
    public bool IsLevyEmployer { get; set; }
    public bool ShowLevyTransparency { get; set; }
}