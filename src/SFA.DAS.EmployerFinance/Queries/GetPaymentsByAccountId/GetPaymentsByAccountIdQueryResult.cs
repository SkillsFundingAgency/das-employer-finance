using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;

public sealed record GetPaymentsByAccountIdQueryResult
{
    public IEnumerable<Payment> Payments { get; init; } = Array.Empty<Payment>();
}