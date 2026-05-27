using SFA.DAS.EmployerFinance.Models.ExpiredFunds;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Extensions;

public static class ExpireFundsExtensions
{
    public static (IDictionary<CalendarPeriod, decimal> LongTerm, IDictionary<CalendarPeriod, decimal> ShortTerm) GetExpiredFunds(
        this IExpiredFunds expiredFundsService,
        IDictionary<CalendarPeriod, decimal> fundsIn,
        IDictionary<CalendarPeriod, decimal> fundsOut,
        IDictionary<CalendarPeriod, decimal> expired,
        int expiryPeriod,
        DateTime today,
        DateTime? policyChangeDate = null,
        int newExpiryPeriod = 12)
    {
        var currentCalendarPeriod = new CalendarPeriod(today.Year, today.Month);

        var expiringFunds = expiredFundsService.GetExpiringFunds(fundsIn, fundsOut, expired, expiryPeriod, expiryEndDate: policyChangeDate);
        var longTerm = expiringFunds
            .Where(ef => ef.Key <= currentCalendarPeriod && ef.Value >= 0 && !expired.Any(e => e.Key == ef.Key && e.Value == ef.Value))
            .ToDictionary(e => e.Key, e => e.Value);

        if (!policyChangeDate.HasValue)
            return (longTerm, new Dictionary<CalendarPeriod, decimal>());

        // fundsIn adjustments have been applied in-place by the long-term run; use a fresh copy so
        // the short-term run's adjustment logic starts clean for its levy cohort
        var fundsInCopy = new Dictionary<CalendarPeriod, decimal>(fundsIn);

        // fundsOut is intentionally shared — the long-term run consumes first, short-term gets the remainder
        var newTermExpiringFunds = expiredFundsService.GetExpiringFunds(fundsInCopy, fundsOut, expired, newExpiryPeriod, expiryStartDate: policyChangeDate);
        var shortTerm = newTermExpiringFunds
            .Where(ef => ef.Key <= currentCalendarPeriod && ef.Value >= 0 && !expired.Any(e => e.Key == ef.Key && e.Value == ef.Value))
            .ToDictionary(e => e.Key, e => e.Value);

        return (longTerm, shortTerm);
    }

    public static IDictionary<CalendarPeriod, decimal> ToCalendarPeriodDictionary(this IEnumerable<LevyFundsIn> levyFundsIn)
    {
        return levyFundsIn.ToDictionary(fund => new CalendarPeriod(fund.CalendarPeriodYear, fund.CalendarPeriodMonth), fund => fund.FundsIn);
    }

    public static IDictionary<CalendarPeriod, decimal> ToCalendarPeriodDictionary(this IEnumerable<PaymentFundsOut> paymentFundsOut)
    {
        return paymentFundsOut.ToDictionary(fund => new CalendarPeriod(fund.CalendarPeriodYear, fund.CalendarPeriodMonth), fund => -fund.FundsOut);
    }

    public static IDictionary<CalendarPeriod, decimal> ToCalendarPeriodDictionary(this IEnumerable<ExpiredFund> expiredFunds)
    {
        return expiredFunds.ToDictionary(fund => new CalendarPeriod(fund.CalendarPeriodYear, fund.CalendarPeriodMonth), fund => -fund.Amount);
    }

    public static IEnumerable<ExpiredFund> ToExpiredFundsList(this IDictionary<CalendarPeriod, decimal> calendarPeriodDictionary)
    {
        return calendarPeriodDictionary.Select(x => new ExpiredFund{ Amount = -x.Value, CalendarPeriodYear = x.Key.Year, CalendarPeriodMonth = x.Key.Month });
    }
}
