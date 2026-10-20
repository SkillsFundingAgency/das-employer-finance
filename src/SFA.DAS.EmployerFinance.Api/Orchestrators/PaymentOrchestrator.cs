using SFA.DAS.EmployerFinance.Api.Extensions;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;
using SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentOrchestrator(IMediator mediator)
{
    public async Task<IEnumerable<PaymentResponse>> GetPaymentDetails(long accountId, long apprenticeshipId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentsByAccountIdQuery(accountId, apprenticeshipId), cancellationToken);
        return result.Payments.Select(p => new PaymentResponse
        {
            Id = p.Id,
            Ukprn = p.Ukprn,
            Uln = p.Uln,
            EmployerAccountId = p.EmployerAccountId,
            ApprenticeshipId = p.ApprenticeshipId,
            DeliveryPeriodMonth = p.DeliveryPeriodMonth,
            DeliveryPeriodYear = p.DeliveryPeriodYear,
            CollectionPeriodMonth = p.CollectionPeriodMonth,
            CollectionPeriodYear = p.CollectionPeriodYear,
            Amount = p.Amount,
            LastUpdatedDate = p.DateImported
        }).ToList();
    }

    public async Task<List<CalendarPeriodAmount>> GetPaymentFundsOut(long accountId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentFundsOutByAccountIdQuery(accountId), cancellationToken);
        return result.FundsOut.ToCalendarPeriodAmounts();
    }
}