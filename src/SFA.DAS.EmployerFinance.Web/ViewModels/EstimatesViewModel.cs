namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class EstimatesViewModel
{
    public DateTime? LastUpdated { get; set; }
    public List<MonthEstimateViewModel> MonthEstimates { get; set; }
}

public class MonthEstimateViewModel
{
    public bool IsProvisional { get; set; }
    public string Period { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal LevyIn { get; set; }
    public decimal LevyOut { get; set; }
    public decimal ExpiredLevy { get; set; }
    public decimal CommittedLearnerCosts { get; set; }
    public decimal CommittedTransfersCosts { get; set; }
}