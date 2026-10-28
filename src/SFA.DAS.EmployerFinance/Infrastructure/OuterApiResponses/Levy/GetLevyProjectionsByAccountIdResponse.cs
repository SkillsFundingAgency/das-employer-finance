using System.Text.Json.Serialization;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

public sealed record GetLevyProjectionsByAccountIdResponse
{
    public long AccountId { get; init; }
    [JsonPropertyName("latestLevyDeclarationInDate")]
    public DateTime LastRefreshDateTime { get; set; }
    public GetLevySummaryByAccountIdResponse Summary { get; set; }
    public List<MonthlyFundingBreakdown> Projections { get; init; } = [];
}

public sealed record MonthlyFundingBreakdown
{
    public long EmployerAccountId { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public decimal ClosingBalance { get; set; }
    public required LevyInSummary LevyInSummary { get; init; }
    public required LevyOutSummary LevyOutSummary { get; init; }
}

public sealed record LevyOutSummary
{
    public decimal Total { get; init; } = 0m;
    public decimal CommittedLearnerCost { get; init; } = 0m;
    public decimal CommittedLearnerFinalPaymentCost { get; init; } = 0m;
    public decimal CommittedTransferOut { get; init; } = 0m;
    public decimal ExpiredLevy { get; init; } = 0m;
    public decimal CommittedTransferOutFinalPaymentCost { get; set; }
}

public sealed record LevyInSummary
{
    public decimal Total { get; init; } = 0m;
    public decimal LevyIn { get; init; } = 0m;
}