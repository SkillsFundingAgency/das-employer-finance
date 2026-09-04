namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public sealed record GetLevySummaryByHashedAccountIdQuery(string HashedAccountId)
    : IRequest<GetLevySummaryByHashedAccountIdQueryResult>;