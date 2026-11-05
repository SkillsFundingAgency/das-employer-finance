using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

public class GetPaymentsByAccountIdAndApprenticeshipIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentsByAccountIdAndApprenticeshipIdQuery, GetPaymentsByAccountIdAndApprenticeshipIdQueryResult>
{
    public async Task<GetPaymentsByAccountIdAndApprenticeshipIdQueryResult> Handle(GetPaymentsByAccountIdAndApprenticeshipIdQuery request, CancellationToken cancellationToken)
    {
        var result = await paymentRepository.GetPayments(request.AccountId, request.ApprenticeshipId, cancellationToken);
        return new GetPaymentsByAccountIdAndApprenticeshipIdQueryResult(result);
    }
}