using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;

public sealed record GetExpiredFundsByAccountIdQueryResult(
    IDictionary<CalendarPeriod, decimal> LongTermExpiredFunds,
    IDictionary<CalendarPeriod, decimal> ShortTermExpiredFunds);
