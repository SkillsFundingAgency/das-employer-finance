using System.Globalization;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public static class EstimatesDownloadColumns
{
    public static readonly string[] Headers =
    [
        "Month", "Levy balance start of month", "Levy in", "Learner costs", "Completion payments",
        "Transfer costs", "Expired levy", "Levy balance end of month"
    ];

    public static string[] GetValues(EstimatesDownloadLine estimate) =>
    [
        estimate.Month.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
        Format(estimate.LevyBalanceStartOfMonth),
        Format(estimate.LevyIn),
        Format(estimate.LearnerCosts),
        Format(estimate.CompletionPayments),
        Format(estimate.TransferCosts),
        Format(estimate.ExpiredLevy),
        Format(estimate.LevyBalanceEndOfMonth)
    ];

    private static string Format(decimal value) => value.ToString("0", NumberFormatInfo.InvariantInfo);
}
