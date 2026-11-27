using System.Globalization;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public static class EstimatesDownloadColumns
{
    private const string NumberFormat = "0.00";

    private static readonly CultureInfo Culture = new("en-GB");

    public const string Disclaimer = "Important: This report contains estimated payments and costs based on your current learner and funding data. The figures are not final and may change due to learner or funding updates, including breaks in learning and withdrawals.";

    // the co-investment columns are only included when at least one month in the download is in co-investment
    public static bool HasCoInvestment(IEnumerable<EstimatesDownloadLine> estimates) =>
        estimates.Any(x => x.CoInvestmentCosts.HasValue);

    public static string[] GetHeaders(bool includeCoInvestment) =>
    [
        "Month", "Levy balance start of month", "Learner costs",
        "Completion payments", "Transfer costs",
        .. includeCoInvestment ? ["Your contribution", "Government contribution"] : Array.Empty<string>(),
        "Levy in", "Expired levy", "Levy balance end of month"
    ];

    public static string[] GetValues(EstimatesDownloadLine estimate, bool includeCoInvestment) =>
    [
        estimate.Month.ToString("MMM-yy", CultureInfo.InvariantCulture),
        Format(estimate.LevyBalanceStartOfMonth),
        Format(estimate.LearnerCosts),
        Format(estimate.CompletionPayments),
        Format(estimate.TransferCosts),
        .. includeCoInvestment ? [Format(estimate.CoInvestmentCosts ?? 0m), Format(estimate.DfeCoInvestmentCost ?? 0m)] : Array.Empty<string>(),
        Format(estimate.LevyIn),
        Format(estimate.ExpiredLevy),
        Format(estimate.LevyBalanceEndOfMonth)
    ];

    public static object[] GetCellValues(EstimatesDownloadLine estimate, bool includeCoInvestment) =>
    [
        estimate.Month.ToDateTime(TimeOnly.MinValue),
        estimate.LevyBalanceStartOfMonth,
        estimate.LearnerCosts,
        estimate.CompletionPayments,
        estimate.TransferCosts,
        .. includeCoInvestment ? [estimate.CoInvestmentCosts ?? 0m, estimate.DfeCoInvestmentCost ?? 0m] : Array.Empty<object>(),
        estimate.LevyIn,
        estimate.ExpiredLevy,
        estimate.LevyBalanceEndOfMonth
    ];

    private static string Format(decimal value) => value.ToString(NumberFormat, Culture);
}
