namespace SFA.DAS.EmployerFinance.Queries.GetLevySummary;

public sealed record GetLevySummaryQuery(string HashedAccountId)
    : IRequest<GetLevySummaryResponse>;