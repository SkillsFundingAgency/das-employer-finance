namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;

public sealed record GetPaymentsByAccountIdQuery(long AccountId, long ApprenticeshipId) 
    : IRequest<GetPaymentsByAccountIdQueryResult>;