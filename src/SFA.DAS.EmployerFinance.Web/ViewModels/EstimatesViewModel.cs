using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class EstimatesViewModel
{
    public string HashedAccountId { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public bool HasProjection => LastUpdatedUtc != DateTime.MinValue;
    public string LastUpdatedUtcLabel => $"Last updated at {LastUpdatedUtc.ToGmtStandardTime().ToGdsTimeFormatFull()} on {LastUpdatedUtc.ToGmtStandardTime().ToGdsFormatFull()}";
    public List<MonthEstimateViewModel> MonthEstimates { get; set; }
}

public class MonthEstimateViewModel
{
    public string Period { get; set; }
    public decimal LevyIn { get; set; }
    //This is for UI only and will slightly differ from whats in the database.
    public decimal LevyOut => ExpiredLevy + CommittedLearnerFinalPaymentCost + CommittedTransferCosts + CommittedLearnerCosts;
    public bool InCoInvestment => CoInvestmentCosts.HasValue;
    public bool ShowCoInvestmentBanner => CoInvestmentCosts is > 0;
    public decimal ClosingLevyBalance { get; set; } = 0;
    public decimal ExpiredLevy { get; set; } = 0;
    public decimal CommittedLearnerCosts { get; set; } = 0;
    public decimal CommittedTransferCosts { get; set; } = 0;
    public decimal CommittedLearnerFinalPaymentCost { get; set; } = 0;
    public decimal CoInvestmentTotal => (CoInvestmentCosts ?? 0) + DfeCoInvestmentCost;
    public decimal? CoInvestmentCosts { get; set; }
    public decimal DfeCoInvestmentCost { get; set; }
}