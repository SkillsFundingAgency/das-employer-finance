namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class FinanceDashboardV2ViewModel
{
    public string HashedAccountId { get; set; }
    public decimal CurrentLevyFunds { get; set; }
    public decimal TotalLevyDeclaredLast12Months { get; set; }
    public decimal TotalLevySpentLast12Months { get; set; }
    public decimal TotalLevyExpiredLast12Months { get; set; }
    public decimal TotalCommittedLearnerCosts { get; set; }
    public decimal TotalCommittedTransfersCosts { get; set; }
    public bool IsLevyEmployer { get; set; }
    public bool ShowLevyTransparency { get; set; }
}