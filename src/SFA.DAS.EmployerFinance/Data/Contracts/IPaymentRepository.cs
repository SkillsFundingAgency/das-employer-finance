using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IPaymentRepository
{
    Task<Payment> GetPayment(long accountId, long apprenticeshipId, CancellationToken cancellationToken);
}