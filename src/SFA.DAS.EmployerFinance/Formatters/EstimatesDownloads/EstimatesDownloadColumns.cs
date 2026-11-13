using System.Globalization;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public static class EstimatesDownloadColumns
{
    private const string NumberFormat = "0.00";

    private static readonly CultureInfo Culture = new("en-GB");

    public static readonly string[] Headers =
    [
        "Month", "Starting levy", "Levy in", "Paid by levy", "Transfers sent",
        "Completion payments", "Expiry", "Remaining levy", "Co-investment"
    ];

    public static string[] GetValues(EstimatesDownloadLine estimate) =>
    [
        estimate.Month.ToString("MMM-yy", CultureInfo.InvariantCulture),
        Format(estimate.LevyBalanceStartOfMonth),
        Format(estimate.LevyIn),
        Format(estimate.LearnerCosts),
        Format(estimate.TransferCosts),
        Format(estimate.CompletionPayments),
        Format(estimate.ExpiredLevy),
        Format(estimate.LevyBalanceEndOfMonth),
        estimate.CoInvestmentCosts?.ToString(NumberFormat, Culture) ?? string.Empty
    ];

    public static object[] GetCellValues(EstimatesDownloadLine estimate) =>
    [
        estimate.Month.ToDateTime(TimeOnly.MinValue),
        estimate.LevyBalanceStartOfMonth,
        estimate.LevyIn,
        estimate.LearnerCosts,
        estimate.TransferCosts,
        estimate.CompletionPayments,
        estimate.ExpiredLevy,
        estimate.LevyBalanceEndOfMonth,
        estimate.CoInvestmentCosts
    ];

    private static string Format(decimal value) => value.ToString(NumberFormat, Culture);
}
