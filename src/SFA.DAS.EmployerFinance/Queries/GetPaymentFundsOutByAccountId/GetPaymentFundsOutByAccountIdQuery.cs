namespace SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;

public sealed record GetPaymentFundsOutByAccountIdQuery(long AccountId)
    : IRequest<GetPaymentFundsOutByAccountIdQueryResult>;
