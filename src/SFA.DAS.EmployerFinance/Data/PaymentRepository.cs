using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using FundingSource = SFA.DAS.Provider.Events.Api.Types.FundingSource;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class PaymentRepository(Lazy<EmployerFinanceDbContext> db) : IPaymentRepository
{
    public async Task<List<Payment>> GetPayments(long accountId,
        long apprenticeshipId,
        CancellationToken cancellationToken)
    {
        // Transfer payments are recorded against the receiving account, so the sending account
        // gets the levy transfer payments for the apprenticeships it funds
        var transferSender = await db.Value.AccountTransfers
            .FirstOrDefaultAsync(t => t.SenderAccountId == accountId && t.ApprenticeshipId == apprenticeshipId, cancellationToken);

        if (transferSender != null)
        {
            accountId = transferSender.ReceiverAccountId;
        }
        
        var transactions = db.Value.Transactions
            .Where(t => t.AccountId == accountId && t.TransactionType == TransactionItemType.Payment);
        
        // An apprenticeship has a payment per period, and DateImported comes from the matching
        // provider payment transaction. A subquery rather than a join keeps one row per payment.
        return await db.Value.Payments
            .Where(p => p.ApprenticeshipId == apprenticeshipId
                        && p.EmployerAccountId == accountId)
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
                FundingSource = payment.FundingSource,
                TransactionType = payment.TransactionType,
                Amount = payment.Amount,
                DateImported = transactions
                    .Where(t => t.UkPrn == payment.Ukprn && t.PeriodEnd == payment.PeriodEnd)
                    .Max(t => (DateTime?)t.TransactionDate) ?? (DateTime?)payment.DateImported ?? DateTime.MinValue
            })
            .ToListAsync(cancellationToken);
    }

    public Task<PeriodEnd> GetPeriodEnd(int calendarPeriodYear, int calendarPeriodMonth, CancellationToken cancellationToken) =>
        db.Value.PeriodEnds
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CalendarPeriodYear == calendarPeriodYear && p.CalendarPeriodMonth == calendarPeriodMonth, cancellationToken);

    public Task<List<Payment>> GetAccountPaymentsForPeriodEnd(long accountId, string periodEndId, CancellationToken cancellationToken) =>
        db.Value.Payments
            .AsNoTracking()
            .Where(p => p.EmployerAccountId == accountId && p.PeriodEnd == periodEndId)
            .ToListAsync(cancellationToken);

    public Task<List<Payment>> GetTransferOutPaymentsForPeriodEnd(long senderAccountId, string periodEndId, CancellationToken cancellationToken)
    {
        var apprenticeshipIds = db.Value.AccountTransfers
            .Where(t => t.SenderAccountId == senderAccountId && t.PeriodEnd == periodEndId)
            .Select(t => t.ApprenticeshipId);

        return db.Value.Payments
            .AsNoTracking()
            .Where(p => p.PeriodEnd == periodEndId && apprenticeshipIds.Contains(p.ApprenticeshipId))
            .ToListAsync(cancellationToken);
    }
}