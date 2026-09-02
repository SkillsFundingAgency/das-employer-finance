using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public sealed record GetLevySummaryQueryResult
{
    public LevySummary Summary { get; init; }
}