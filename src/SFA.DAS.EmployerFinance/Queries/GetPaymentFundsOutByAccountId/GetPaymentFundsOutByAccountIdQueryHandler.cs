using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;

public class GetPaymentFundsOutByAccountIdQueryHandler(IPaymentFundsOutRepository paymentFundsOutRepository)
    : IRequestHandler<GetPaymentFundsOutByAccountIdQuery, GetPaymentFundsOutByAccountIdQueryResult>
{
    public async Task<GetPaymentFundsOutByAccountIdQueryResult> Handle(GetPaymentFundsOutByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var fundsOut = await paymentFundsOutRepository.GetPaymentFundsOut(request.AccountId);
        return new GetPaymentFundsOutByAccountIdQueryResult(fundsOut.ToCalendarPeriodDictionary());
    }
}
