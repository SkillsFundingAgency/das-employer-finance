namespace SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;

public sealed record GetExpiredFundsByAccountIdQuery(long AccountId)
    : IRequest<GetExpiredFundsByAccountIdQueryResult>;
