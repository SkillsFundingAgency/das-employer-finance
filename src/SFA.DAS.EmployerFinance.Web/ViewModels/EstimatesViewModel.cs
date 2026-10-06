using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class EstimatesViewModel
{
    public DateTime LastUpdatedUtc { get; set; }
    public string LastUpdatedUtcLabel => $"Updated at {LastUpdatedUtc.ToGmtStandardTime().ToGdsTimeFormatFull()} on {LastUpdatedUtc.ToGmtStandardTime().ToGdsFormatFull()}";
    public List<MonthEstimateViewModel> MonthEstimates { get; set; }
}

public class MonthEstimateViewModel
{
    public string Period { get; set; }
    public decimal LevyIn { get; set; }
    public decimal LevyOut { get; set; }
    public decimal ClosingLevyBalance { get; set; } = 0;
    public decimal ExpiredLevy { get; set; } = 0;
    public decimal CommittedLearnerCosts { get; set; } = 0;
    public decimal CommittedTransferCosts { get; set; } = 0;
}