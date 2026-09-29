namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

public sealed record GetLevySummaryByAccountIdQuery(long AccountId)
    : IRequest<GetLevySummaryByAccountIdQueryResult>;