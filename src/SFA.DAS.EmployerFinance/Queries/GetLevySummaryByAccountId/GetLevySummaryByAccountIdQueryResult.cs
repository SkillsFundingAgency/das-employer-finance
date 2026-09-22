using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

public sealed record GetLevySummaryByAccountIdQueryResult
{
    public LevySummary Summary { get; init; }
}