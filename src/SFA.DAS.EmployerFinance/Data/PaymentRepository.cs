using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class PaymentRepository(Lazy<EmployerFinanceDbContext> db) : IPaymentRepository
{
    public async Task<Payment> GetPayment(long accountId,
        long apprenticeshipId,
        CancellationToken cancellationToken)
    {
        return await db.Value.Payments
            .Where(p => p.EmployerAccountId == accountId)
            .Join(
                db.Value.Transactions.Where(t => t.AccountId == accountId),
                payment => new { payment.Ukprn, payment.PeriodEnd },
                transaction => new { Ukprn = transaction.UkPrn.GetValueOrDefault(), transaction.PeriodEnd },
                (payment, transaction) => new Payment
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
                    DateImported = transaction.TransactionDate
                })
            .FirstOrDefaultAsync(cancellationToken); // One to one - one learner gets one payment record 
    }
}