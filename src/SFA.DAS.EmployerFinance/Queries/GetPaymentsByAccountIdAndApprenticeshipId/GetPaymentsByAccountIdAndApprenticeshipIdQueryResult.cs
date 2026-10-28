using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

public sealed record GetPaymentsByAccountIdAndApprenticeshipIdQueryResult(IReadOnlyCollection<Payment> Payments);