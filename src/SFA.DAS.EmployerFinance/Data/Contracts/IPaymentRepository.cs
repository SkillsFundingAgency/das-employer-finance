using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IPaymentRepository
{
    Task<List<Payment>> GetPayments(long accountId, long apprenticeshipId, CancellationToken cancellationToken);
    Task<PeriodEnd> GetPeriodEnd(int calendarPeriodYear, int calendarPeriodMonth, CancellationToken cancellationToken);
    Task<List<Payment>> GetAccountPaymentsForPeriodEnd(long accountId, string periodEndId, CancellationToken cancellationToken);
    Task<List<Payment>> GetTransferOutPaymentsForPeriodEnd(long senderAccountId, string periodEndId, CancellationToken cancellationToken);
}