using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using FundingSource = SFA.DAS.Provider.Events.Api.Types.FundingSource;

namespace SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;

public class GetCollectionPeriodPaymentsByAccountIdQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetCollectionPeriodPaymentsByAccountIdQuery, GetCollectionPeriodPaymentsByAccountIdQueryResult>
{
    private static readonly FundingSource[] AccountFundingSources = [FundingSource.Levy, FundingSource.CoInvestedEmployer];

    public async Task<GetCollectionPeriodPaymentsByAccountIdQueryResult> Handle(GetCollectionPeriodPaymentsByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var periodEnd = await paymentRepository.GetPeriodEnd(request.Year, request.Month, cancellationToken);
        if (periodEnd == null)
        {
            return new GetCollectionPeriodPaymentsByAccountIdQueryResult(false, []);
        }

        var accountPayments = await paymentRepository.GetAccountPaymentsForPeriodEnd(request.AccountId, periodEnd.PeriodEndId, cancellationToken);
        var transferOutPayments = await paymentRepository.GetTransferOutPaymentsForPeriodEnd(request.AccountId, periodEnd.PeriodEndId, cancellationToken);

        var payments = accountPayments
            .Where(p => AccountFundingSources.Contains(p.FundingSource))
            .Concat(transferOutPayments.Where(p => p.FundingSource == FundingSource.LevyTransfer));

        return new GetCollectionPeriodPaymentsByAccountIdQueryResult(true, [.. Summarise(payments)]);
    }

    private static IEnumerable<CollectionPeriodPaymentTotal> Summarise(IEnumerable<Payment> payments) =>
        payments
            .GroupBy(p => (p.FundingSource, p.TransactionType))
            .Select(g => new CollectionPeriodPaymentTotal(g.Key.FundingSource, g.Key.TransactionType, g.Sum(p => p.Amount)));
}
