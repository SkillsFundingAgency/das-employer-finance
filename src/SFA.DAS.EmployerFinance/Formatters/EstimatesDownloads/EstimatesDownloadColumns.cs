using System.Globalization;
using SFA.DAS.EmployerFinance.Models.Estimates;

namespace SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;

public static class EstimatesDownloadColumns
{
    public const string CurrencyFormat = "C2";
    public const string NumberFormat = "0.00";

    private static readonly CultureInfo Culture = new("en-GB");

    public static readonly string[] Headers =
    [
        "Month", "Starting levy", "Levy in", "Paid by levy", "Transfers sent",
        "Completion payments", "Expiry", "Remaining levy", "Co-investment"
    ];

    public static string[] GetValues(EstimatesDownloadLine estimate, string moneyFormat) =>
    [
        estimate.Month.ToString("MMM-yy", CultureInfo.InvariantCulture),
        Format(estimate.LevyBalanceStartOfMonth, moneyFormat),
        Format(estimate.LevyIn, moneyFormat),
        Format(estimate.LearnerCosts, moneyFormat),
        Format(estimate.TransferCosts, moneyFormat),
        Format(estimate.CompletionPayments, moneyFormat),
        Format(estimate.ExpiredLevy, moneyFormat),
        Format(estimate.LevyBalanceEndOfMonth, moneyFormat),
        FormatCoInvestment(estimate.CoInvestmentCosts, moneyFormat)
    ];

    private static string Format(decimal value, string moneyFormat) => value.ToString(moneyFormat, Culture);

    private static string FormatCoInvestment(decimal value, string moneyFormat) => Math.Round(value, 2) > 0 ? Format(value, moneyFormat) : string.Empty;
}
