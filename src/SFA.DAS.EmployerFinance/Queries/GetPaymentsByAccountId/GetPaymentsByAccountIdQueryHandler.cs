using SFA.DAS.EmployerFinance.Data.Contracts;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;

public class GetPaymentsByAccountIdQueryHandler(IPaymentRepository paymentRepository) : IRequestHandler<GetPaymentsByAccountIdQuery, GetPaymentsByAccountIdQueryResult>
{
    public async Task<GetPaymentsByAccountIdQueryResult> Handle(GetPaymentsByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var result = await paymentRepository.GetPayment(request.AccountId, request.ApprenticeshipId, cancellationToken);
        return new GetPaymentsByAccountIdQueryResult(result);
    }
}