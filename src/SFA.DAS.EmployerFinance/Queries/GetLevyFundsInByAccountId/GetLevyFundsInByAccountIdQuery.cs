namespace SFA.DAS.EmployerFinance.Queries.GetLevyFundsInByAccountId;

public sealed record GetLevyFundsInByAccountIdQuery(long AccountId)
    : IRequest<GetLevyFundsInByAccountIdQueryResult>;
