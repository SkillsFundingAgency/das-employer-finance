using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFinance.Data;

[ExcludeFromCodeCoverage]
public class PaymentRepository(Lazy<EmployerFinanceDbContext> db) : IPaymentRepository
{
    public async Task<IEnumerable<Payment>> GetPayments(long accountId, long apprenticeshipId, CancellationToken cancellationToken)
    {
        return await db.Value.Payments
            .Where(p => p.EmployerAccountId == accountId && p.ApprenticeshipId == apprenticeshipId)
            .ToListAsync(cancellationToken);
    }
}