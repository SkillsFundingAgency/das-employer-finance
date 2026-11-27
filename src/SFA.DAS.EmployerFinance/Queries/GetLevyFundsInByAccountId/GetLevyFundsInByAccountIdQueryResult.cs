using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyFundsInByAccountId;

public sealed record GetLevyFundsInByAccountIdQueryResult(IDictionary<CalendarPeriod, decimal> FundsIn);
