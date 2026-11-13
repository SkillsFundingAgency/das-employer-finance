using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Estimates;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;

public class GetEstimatesDownloadQueryHandler(
    IOuterApiService outerApiService,
    IEstimatesFormatterFactory estimatesFormatterFactory)
    : IRequestHandler<GetEstimatesDownloadQuery, GetEstimatesDownloadResponse>
{
    public async Task<GetEstimatesDownloadResponse> Handle(GetEstimatesDownloadQuery message, CancellationToken cancellationToken)
    {
        var projections = await outerApiService.GetLevyProjectionsForDownload(message.AccountId, cancellationToken);

        var formatter = estimatesFormatterFactory.GetEstimatesFormatterByType(message.DownloadFormat);

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
                LevyIn = projection.LevyInSummary.Total,
                LearnerCosts = projection.LevyOutSummary.CommittedLearnerCost * -1,
                CompletionPayments = projection.LevyOutSummary.CommittedLearnerFinalPaymentCost * -1,
                TransferCosts = (projection.LevyOutSummary.CommittedTransferOut + projection.LevyOutSummary.CommittedTransferOutFinalPaymentCost) * -1,
                ExpiredLevy = projection.LevyOutSummary.ExpiredLevy * -1,
                LevyBalanceEndOfMonth = NotBelowZero(projection.ClosingBalance),
                CoInvestmentCosts = projection.CoInvestmentCosts
            });

            startOfMonthBalance = projection.ClosingBalance;
        }

        return lines;
    }

    private static decimal NotBelowZero(decimal balance) => Math.Max(0m, balance);
}
