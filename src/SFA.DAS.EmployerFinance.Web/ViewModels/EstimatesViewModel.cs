namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class EstimatesViewModel
{
    public DateTime LastUpdatedUtc { get; set; }
    public List<MonthEstimateViewModel> MonthEstimates { get; set; }
}

public class MonthEstimateViewModel
{
    public string Period { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal LevyIn { get; set; }
    public decimal LevyOut => ExpiredLevy + CommittedLearnerCosts + CommittedTransferCosts;

    public decimal ExpiredLevy { get; set; } = 0;
    public decimal CommittedLearnerCosts { get; set; } = 0;
    public decimal CommittedTransferCosts { get; set; } = 0;
}