namespace SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;

public sealed record GetCollectionPeriodPaymentsByAccountIdQuery(long AccountId, int Year, int Month)
    : IRequest<GetCollectionPeriodPaymentsByAccountIdQueryResult>;
