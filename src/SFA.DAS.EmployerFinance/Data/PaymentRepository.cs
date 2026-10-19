using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class PaymentRepository(Lazy<EmployerFinanceDbContext> db) : IPaymentRepository
{
    public async Task<List<Payment>> GetPayments(long accountId,
        long apprenticeshipId,
        CancellationToken cancellationToken)
    {
        var transactions = db.Value.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionType == TransactionItemType.Payment);

        // An apprenticeship has a payment per period, and DateImported comes from the matching
        // provider payment transaction. A subquery rather than a join keeps one row per payment.
        return await db.Value.Payments
            .Where(p => p.EmployerAccountId == accountId && p.ApprenticeshipId == apprenticeshipId)
            .Select(payment => new Payment
            {
                Id = payment.Id,
                Ukprn = payment.Ukprn,
                Uln = payment.Uln,
                PeriodEnd = payment.PeriodEnd,
                EmployerAccountId = payment.EmployerAccountId,
                ApprenticeshipId = payment.ApprenticeshipId,
                DeliveryPeriodMonth = payment.DeliveryPeriodMonth,
                DeliveryPeriodYear = payment.DeliveryPeriodYear,
                CollectionPeriodMonth = payment.CollectionPeriodMonth,
                CollectionPeriodYear = payment.CollectionPeriodYear,
                Amount = payment.Amount,
                DateImported = transactions
                    .Where(t => t.UkPrn == payment.Ukprn && t.PeriodEnd == payment.PeriodEnd)
                    .Max(t => (DateTime?)t.TransactionDate) ?? (DateTime?)payment.DateImported ?? DateTime.MinValue
            })
            .ToListAsync(cancellationToken);
    }
}