using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Api.Extensions;

public static class CalendarPeriodDictionaryExtensions
{
    // CalendarPeriod isn't a supported JSON dictionary key, so flatten to a list ordered by period
    public static List<CalendarPeriodAmount> ToCalendarPeriodAmounts(this IDictionary<CalendarPeriod, decimal> calendarPeriodDictionary) =>
        calendarPeriodDictionary
            .OrderBy(x => x.Key)
            .Select(x => new CalendarPeriodAmount { Year = x.Key.Year, Month = x.Key.Month, Amount = x.Value })
            .ToList();
}
