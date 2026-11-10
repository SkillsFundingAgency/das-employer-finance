using SFA.DAS.EmployerFinance.Formatters.EstimatesDownloads;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Models.Estimates;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;

public class GetEstimatesDownloadQueryHandler(
    IOuterApiService outerApiService,
    IEnumerable<IEstimatesFormatter> formatters)
    : IRequestHandler<GetEstimatesDownloadQuery, GetEstimatesDownloadResponse>
{
    public async Task<GetEstimatesDownloadResponse> Handle(GetEstimatesDownloadQuery message, CancellationToken cancellationToken)
    {
        var projections = await outerApiService.GetLevyProjectionsForDownload(message.AccountId, cancellationToken);

        var formatter = formatters.Single(x => x.DownloadFormatType == message.DownloadFormat);

        return new GetEstimatesDownloadResponse
        {
            FileData = formatter.GetFileData(CreateLines(projections)),
            FileExtension = formatter.FileExtension,
            MimeType = formatter.MimeType
        };
    }

    private static List<EstimatesDownloadLine> CreateLines(GetLevyProjectionsByAccountIdResponse projections)
    {
        var lines = new List<EstimatesDownloadLine>(projections.Projections.Count);

        var startOfMonthBalance = projections.Summary?.CurrentLevyFunds ?? 0m;

        foreach (var projection in projections.Projections)
        {
            lines.Add(new EstimatesDownloadLine
            {
                Month = new DateOnly(projection.Year, projection.Month, 1),
                LevyBalanceStartOfMonth = NotBelowZero(startOfMonthBalance),
                LevyIn = Math.Ceiling(projection.LevyInSummary.Total),
                LearnerCosts = Math.Ceiling(projection.LevyOutSummary.CommittedLearnerCost) * -1,
                CompletionPayments = Math.Ceiling(projection.LevyOutSummary.CommittedLearnerFinalPaymentCost) * -1,
                TransferCosts = Math.Ceiling(projection.LevyOutSummary.CommittedTransferOut + projection.LevyOutSummary.CommittedTransferOutFinalPaymentCost) * -1,
                ExpiredLevy = Math.Ceiling(projection.LevyOutSummary.ExpiredLevy) * -1,
                LevyBalanceEndOfMonth = NotBelowZero(projection.ClosingBalance)
            });

            startOfMonthBalance = projection.ClosingBalance;
        }

        return lines;
    }

    private static decimal NotBelowZero(decimal balance) => Math.Max(0m, Math.Ceiling(balance));
}
