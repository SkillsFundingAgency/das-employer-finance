namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class EstimatesViewModel
{
    public List<MonthEstimateViewModel> MonthEstimates { get; set; }
}

public class MonthEstimateViewModel
{
    public int Month { get; set; }
    public string MonthName { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal LevyIn { get; set; }
    public decimal LevyOut { get; set; }
    public decimal ExpiredLevy { get; set; }
    public decimal CommitedLearnerCosts { get; set; }
    public decimal CommitedTransfersCosts { get; set; }
}