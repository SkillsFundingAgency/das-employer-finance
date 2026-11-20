using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;

public sealed record GetPaymentFundsOutByAccountIdQueryResult(IDictionary<CalendarPeriod, decimal> FundsOut);
