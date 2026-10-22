using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountId;

public sealed record GetPaymentsByAccountIdQueryResult(IReadOnlyCollection<Payment> Payments);