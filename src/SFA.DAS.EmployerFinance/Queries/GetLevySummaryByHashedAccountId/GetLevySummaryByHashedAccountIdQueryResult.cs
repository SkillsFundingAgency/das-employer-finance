using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public sealed record GetLevySummaryByHashedAccountIdQueryResult
{
    public LevySummary Summary { get; init; }
}