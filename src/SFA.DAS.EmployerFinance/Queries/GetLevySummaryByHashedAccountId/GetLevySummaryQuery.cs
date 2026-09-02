namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public sealed record GetLevySummaryQuery(string HashedAccountId)
    : IRequest<GetLevySummaryQueryResult>;