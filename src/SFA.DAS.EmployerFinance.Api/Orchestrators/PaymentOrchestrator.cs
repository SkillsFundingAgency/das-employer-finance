using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentOrchestrator(IMediator mediator)
{
    public async Task<PaymentResponse> GetPaymentDetails(long accountId, long apprenticeshipId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPaymentsByAccountIdQuery(accountId, apprenticeshipId), cancellationToken);
        return new PaymentResponse
        {
            Id = result.Payment.Id,
            Ukprn = result.Payment.Ukprn,
            Uln = result.Payment.Uln,
            EmployerAccountId = result.Payment.EmployerAccountId,
            ApprenticeshipId = result.Payment.ApprenticeshipId,
            DeliveryPeriodMonth = result.Payment.DeliveryPeriodMonth,
            DeliveryPeriodYear = result.Payment.DeliveryPeriodYear,
            CollectionPeriodMonth = result.Payment.CollectionPeriodMonth,
            CollectionPeriodYear = result.Payment.CollectionPeriodYear,
            Amount = result.Payment.Amount,
            LastUpdatedDate = result.Payment.DateImported
        };
    }
}