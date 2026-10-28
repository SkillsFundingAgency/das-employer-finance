namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

public sealed record GetPaymentsByAccountIdAndApprenticeshipIdQuery(long AccountId, long ApprenticeshipId) 
    : IRequest<GetPaymentsByAccountIdAndApprenticeshipIdQueryResult>;