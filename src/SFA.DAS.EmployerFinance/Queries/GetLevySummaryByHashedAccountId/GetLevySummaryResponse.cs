using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public sealed record GetLevySummaryResponse
{
    public LevySummary Summary { get; set; }
}