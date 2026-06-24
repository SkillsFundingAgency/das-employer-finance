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
        IDictionary<CalendarPeriod, decimal> longTermExpired,
        IDictionary<CalendarPeriod, decimal> shortTermExpired,
        int expiryPeriod,
        DateTime today,
        DateTime? policyChangeDate = null,
        int newExpiryPeriod = 12)
    {
        var currentCalendarPeriod = new CalendarPeriod(today.Year, today.Month);

        // Pre-filter each levy expiry amount so CalculateAndApplyAdjustmentsToTotals inside GetExpiringFunds
        // only operates on the relevant levy periods and never double-adjusts across runs.
        var policyChangePeriod = policyChangeDate.HasValue
            ? new CalendarPeriod(policyChangeDate.Value.Year, policyChangeDate.Value.Month)
            : null;

        var longTermFundsIn = policyChangePeriod != null
            ? fundsIn.Where(c => c.Key < policyChangePeriod).ToDictionary(c => c.Key, c => c.Value)
            : new Dictionary<CalendarPeriod, decimal>(fundsIn);

        var expiringFunds = expiredFundsService.GetExpiringFunds(longTermFundsIn, fundsOut, longTermExpired, expiryPeriod);
        var longTerm = expiringFunds
            .Where(ef => ef.Key <= currentCalendarPeriod && ef.Value >= 0 && !longTermExpired.Any(e => e.Key == ef.Key && e.Value == ef.Value))
            .ToDictionary(e => e.Key, e => e.Value);

        if (policyChangePeriod == null)
            return (longTerm, new Dictionary<CalendarPeriod, decimal>());

        // Filter from the original fundsIn (not the mutated longTermFundsIn copy) so the short-term
        // starts with clean, unadjusted values for its own levy periods.
        // fundsOut is intentionally shared — the long-term run consumes first, short-term gets the remainder.
        var shortTermFundsIn = fundsIn
            .Where(c => c.Key >= policyChangePeriod)
            .ToDictionary(c => c.Key, c => c.Value);

        var newTermExpiringFunds = expiredFundsService.GetExpiringFunds(shortTermFundsIn, fundsOut, shortTermExpired, newExpiryPeriod);
        var shortTerm = newTermExpiringFunds
            .Where(ef => ef.Key <= currentCalendarPeriod && ef.Value >= 0 && !shortTermExpired.Any(e => e.Key == ef.Key && e.Value == ef.Value))
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
