using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.UnitTests.Extensions;

[Parallelizable]
public class GetExpiredFundsExtensionTests
{
    [Test]
    public void WithoutPolicyChangeDate_OnlyLongTermExpiredFundsAreReturned()
    {
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var fundsOut = BuildFundsOutCoveringTwentyFourAndTwelveMonthExpiry();
        var today = new DateTime(2023, 6, 1);

        var (longTerm, shortTerm) = expiredFunds.GetExpiredFunds(
            fundsIn, fundsOut,
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: null);

        Assert.That(shortTerm, Is.Empty, "ShortTerm should be empty when there is no policy change date");

        // Only expiry dates up to today (2023-06) are included.
        // Levy 2019-01 to 2021-06 expires 2021-01 to 2023-06 = 30 entries.
        Assert.That(longTerm.Count, Is.EqualTo(30));

        // The 1500 payment at 2022-06 covers levy 2020-06 (expiry 2022-06) in full (500 remaining)
        // and then partially covers levy 2020-07 (expiry 2022-07), leaving 500 to expire.
        Assert.That(longTerm[new CalendarPeriod(2022, 6)], Is.EqualTo(0m));
        Assert.That(longTerm[new CalendarPeriod(2022, 7)], Is.EqualTo(500m));

        // All other periods expire the full 1000.
        var otherEntries = longTerm.Where(e =>
            e.Key != new CalendarPeriod(2022, 6) && e.Key != new CalendarPeriod(2022, 7));
        Assert.That(otherEntries.All(e => e.Value == 1000m), Is.True,
            "Every other period should expire the full 1000");

        Assert.That(longTerm.Values.Sum(), Is.EqualTo(28500m));
    }

    [Test]
    public void WithPolicyChangeDate_LongTermCovers24MonthLevyBeforeChangeDate()
    {
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var fundsOut = BuildFundsOutCoveringTwentyFourAndTwelveMonthExpiry();
        var today = new DateTime(2023, 6, 1);
        var policyChangeDate = new DateTime(2021, 1, 1);

        var (longTerm, _) = expiredFunds.GetExpiredFunds(
            fundsIn, fundsOut,
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // policyChangeDate is the FIRST month of the new (12-month) policy.
        // Long-term cohort: levy < policyChangePeriod → 2019-01 to 2020-12 (24 months).
        // Expiry dates 2021-01 to 2022-12, all ≤ today (2023-06) → count = 24.
        Assert.That(longTerm.Count, Is.EqualTo(24));
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 1)), Is.True, "Earliest 24-month expiry should be present");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2022, 12)), Is.True, "Latest 24-month expiry should be present");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2023, 1)), Is.False, "Levy on or after policyChangeDate (2021-01) is short-term, not long-term");

        // Payment at 2022-06 = 1500: covers 2020-06 levy fully (500 left), then partially covers 2020-07 levy.
        Assert.That(longTerm[new CalendarPeriod(2022, 6)], Is.EqualTo(0m));
        Assert.That(longTerm[new CalendarPeriod(2022, 7)], Is.EqualTo(500m));
        Assert.That(longTerm.Values.Sum(), Is.EqualTo(22500m));
    }

    [Test]
    public void WithPolicyChangeDate_ShortTermCovers12MonthLevyAfterChangeDate()
    {
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var fundsOut = BuildFundsOutCoveringTwentyFourAndTwelveMonthExpiry();
        var today = new DateTime(2023, 6, 1);
        var policyChangeDate = new DateTime(2021, 1, 1);

        var (_, shortTerm) = expiredFunds.GetExpiredFunds(
            fundsIn, fundsOut,
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // Short-term cohort: levy >= policyChangePeriod → 2021-01 to 2021-12 (12 months).
        // Expiry dates 2022-01 to 2022-12, all ≤ today (2023-06) → count = 12.
        // The levy on the policyChangeDate itself (2021-01) IS in short-term (>= semantics).
        Assert.That(shortTerm.Count, Is.EqualTo(12));
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 1)), Is.True, "Levy on policyChangeDate (2021-01) is short-term, expiry 2022-01");
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 12)), Is.True, "Latest 12-month expiry should be present");

        // Payment at 2022-06 was consumed by the long-term run (see FundsOut_ConsumedByLongTermRunFirst test).
        // All 12 short-term periods expire in full: 12 × 1000 = 12000.
        Assert.That(shortTerm.Values.Sum(), Is.EqualTo(12000m));
    }

    [Test]
    public void FundsOut_ConsumedByLongTermRunFirst_LeavingNoneForShortTerm()
    {
        // The payment at 2022-06 sits in a date range that the 24-month run can reach.
        // After the long-term run consumes it, no fundsOut remains for the 12-month run,
        // so the short-term levy expiry for the same month (June 2022) sees the full 1000.
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var fundsOut = BuildFundsOutCoveringTwentyFourAndTwelveMonthExpiry();
        var today = new DateTime(2023, 6, 1);
        var policyChangeDate = new DateTime(2021, 1, 1);

        var (longTerm, shortTerm) = expiredFunds.GetExpiredFunds(
            fundsIn, fundsOut,
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // Long-term run consumed the 2022-06 payment fully.
        Assert.That(longTerm[new CalendarPeriod(2022, 6)], Is.EqualTo(0m),
            "Long-term run should have consumed the 2022-06 payment reducing its own expiry to 0");

        // Short-term run sees 2022-06 payment as already exhausted, so levy 2021-06 expires in full.
        Assert.That(shortTerm[new CalendarPeriod(2022, 6)], Is.EqualTo(1000m),
            "Short-term run should find no remaining payment balance, causing the full 1000 to expire");
    }

    [Test]
    public void FuturePeriods_ArNotIncluded_WhenKeyExceedsToday()
    {
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var today = new DateTime(2022, 6, 1);
        var policyChangeDate = new DateTime(2021, 1, 1);

        var (longTerm, shortTerm) = expiredFunds.GetExpiredFunds(
            fundsIn, new Dictionary<CalendarPeriod, decimal>(),
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // With today = 2022-06 only expiry dates ≤ 2022-06 are included.
        // LongTerm (levy < 2021-01): 2019-01 to 2020-12 → expiry 2021-01 to 2022-12, capped at 2022-06 = 18 entries.
        Assert.That(longTerm.Count, Is.EqualTo(18));
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2022, 6)), Is.True);
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2022, 7)), Is.False,
            "Expiry dates beyond today should be excluded");

        // ShortTerm (levy >= 2021-01): 2021-01 to 2021-06 → expiry 2022-01 to 2022-06 (6 entries).
        Assert.That(shortTerm.Count, Is.EqualTo(6));
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 1)), Is.True, "Levy on policyChangeDate (2021-01) is short-term");
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 6)), Is.True);
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 7)), Is.False,
            "Short-term expiry dates beyond today should be excluded");
    }

    [Test]
    public void ExistingExpiredFunds_AreExcludedFromResults()
    {
        // If a month's expiry has already been recorded, it must not appear in the new results.
        var expiredFunds = new ExpiredFunds();
        var fundsIn = BuildThreeYearFundsIn();
        var today = new DateTime(2023, 6, 1);
        var policyChangeDate = new DateTime(2021, 1, 1);

        // Simulate that 2021-01 and 2021-02 have already been expired (value stored as negative per convention).
        var existingExpired = new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2021, 1), -1000m },
            { new CalendarPeriod(2021, 2), -1000m }
        };

        var (longTerm, _) = expiredFunds.GetExpiredFunds(
            fundsIn, new Dictionary<CalendarPeriod, decimal>(),
            longTermExpired: existingExpired,
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 1)), Is.False,
            "Already-expired 2021-01 should be excluded from new results");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 2)), Is.False,
            "Already-expired 2021-02 should be excluded from new results");

        // Long-term cohort (levy < 2021-01): 24 periods, minus 2 already expired = 22.
        Assert.That(longTerm.Count, Is.EqualTo(22));
    }
    
    [Test]
    public void ShortTermNegativeAdjustment_IsAppliedOnce_NotDoubleAdjusted()
    {
        var expiredFunds = new ExpiredFunds();
        var policyChangeDate = new DateTime(2021, 1, 1);
        var today = new DateTime(2023, 6, 1);

        var fundsIn = new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2019, 1), 1000m },  // long-term: expires 2021-01 after 24 months
            { new CalendarPeriod(2021, 2), 1000m },  // short-term: expires 2022-02 after 12 months
            { new CalendarPeriod(2021, 6), -500m }   // short-term adjustment: should reduce 2021-02 by 500 once
        };

        var (longTerm, shortTerm) = expiredFunds.GetExpiredFunds(
            fundsIn,
            fundsOut: new Dictionary<CalendarPeriod, decimal>(),
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate,
            newExpiryPeriod: 12);

        Assert.That(longTerm[new CalendarPeriod(2021, 1)], Is.EqualTo(1000m),
            "Long-term levy should be unaffected by the short-term adjustment");

        // The adjustment reduces 2021-02 from 1000 to 500. Applied once → 500 expires.
        // Applied twice (the bug) → 0 expires.
        Assert.That(shortTerm[new CalendarPeriod(2022, 2)], Is.EqualTo(500m),
            "Short-term adjustment should be applied exactly once, leaving 500 to expire");
    }

    [Test]
    public void MonthByMonthRun_September2027_ShouldHaveBothLongTermAndShortTermExpiry()
    {
        // Simulate what the DraftExpireAccountFundsCommandHandler does month by month.
        // Each month it:
        //   1. Reads existing stored draft records (type=5 long-term, type=6 short-term)
        //   2. Calls GetExpiredFunds with the accumulated context
        //   3. Stores only the current period's expiry

        var expiredFundsService = new ExpiredFunds();
        var policyChangeDate = new DateTime(2026, 8, 1);

        // Funds in: 100/month from 2025-04 to 2026-09 (inclusive)
        var fundsIn = new Dictionary<CalendarPeriod, decimal>();
        for (var d = new DateTime(2025, 4, 1); d <= new DateTime(2026, 9, 1); d = d.AddMonths(1))
            fundsIn[new CalendarPeriod(d.Year, d.Month)] = 100m;

        var fundsOut = new Dictionary<CalendarPeriod, decimal>();

        // Simulate stored draft records (like the DB table)
        var storedType5 = new List<(int Year, int Month, decimal Amount)>();
        var storedType6 = new List<(int Year, int Month, decimal Amount)>();

        var runMonths = Enumerable.Range(0, 6).Select(i => new DateTime(2027, 4, 1).AddMonths(i)).ToList();

        foreach (var runMonth in runMonths)
        {
            // Handler sets now = DateTo with day 28
            var today = new DateTime(runMonth.Year, runMonth.Month, 28);

            // Build existing expired fund dicts from stored records (mirrors ToCalendarPeriodDictionary)
            var longTermExpired = storedType5.ToDictionary(r => new CalendarPeriod(r.Year, r.Month), r => -r.Amount);
            var shortTermExpired = storedType6.ToDictionary(r => new CalendarPeriod(r.Year, r.Month), r => -r.Amount);

            var (longTermResult, shortTermResult) = expiredFundsService.GetExpiredFunds(
                fundsIn,
                fundsOut,
                longTermExpired,
                shortTermExpired,
                expiryPeriod: 24,
                today: today,
                policyChangeDate: policyChangeDate,
                newExpiryPeriod: 12);

            var currentPeriod = new CalendarPeriod(runMonth.Year, runMonth.Month);
            if (!longTermResult.ContainsKey(currentPeriod)) longTermResult[currentPeriod] = 0;
            if (!shortTermResult.ContainsKey(currentPeriod)) shortTermResult[currentPeriod] = 0;

            // Store current period only (mirrors handler's CreateDraft call)
            // Amount = -value (mirrors ToExpiredFundsList), read back as -Amount = value (mirrors ToCalendarPeriodDictionary)
            storedType5.Add((runMonth.Year, runMonth.Month, -longTermResult[currentPeriod]));
            storedType6.Add((runMonth.Year, runMonth.Month, -shortTermResult[currentPeriod]));
        }

        var sept5 = storedType5.First(r => r.Year == 2027 && r.Month == 9);
        var sept6 = storedType6.First(r => r.Year == 2027 && r.Month == 9);

        Assert.That(-sept5.Amount, Is.EqualTo(100m), $"September 2027 long-term expiry should be 100, got {-sept5.Amount}");
        Assert.That(-sept6.Amount, Is.EqualTo(100m), $"September 2027 short-term expiry should be 100, got {-sept6.Amount}");
    }

    [Test]
    public void PolicyChangeDate_WhenOffByOneMonthLate_August2026LevyIsLongTermInsteadOfShortTerm()
    {
        // policyChangeDate = first month of new (short-term) policy.
        // Levies >= policyChangeDate go into short-term (12-month); levies < policyChangeDate go into long-term (24-month).
        //
        // Correct date: 2026-08-01 → 2026-08 and 2026-09 levies are short-term.
        // Wrong date:   2026-09-01 → only 2026-09 is short-term; 2026-08 is misclassified as long-term
        //               and won't expire until 2028-08 instead of 2027-08.
        var expiredFundsService = new ExpiredFunds();
        var wrongPolicyChangeDate = new DateTime(2026, 9, 1); // should be 2026-08-01

        var fundsIn = new Dictionary<CalendarPeriod, decimal>();
        for (var d = new DateTime(2025, 4, 1); d <= new DateTime(2026, 9, 1); d = d.AddMonths(1))
            fundsIn[new CalendarPeriod(d.Year, d.Month)] = 100m;

        var today = new DateTime(2027, 9, 28);
        var (longTerm, shortTerm) = expiredFundsService.GetExpiredFunds(
            fundsIn,
            fundsOut: new Dictionary<CalendarPeriod, decimal>(),
            longTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            shortTermExpired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: wrongPolicyChangeDate,
            newExpiryPeriod: 12);

        // With 2026-09 as the cutover (one month late), levy 2026-08 is misclassified as long-term.
        // It will not expire until 2028-08 (24 months), so August 2027 has no short-term expiry.
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2027, 8)), Is.False,
            "2026-08 levy should be misclassified as long-term, producing no short-term expiry for 2027-08");

        // Levy 2026-09 is correctly in short-term (>= 2026-09) and expires after 12 months = 2027-09.
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2027, 9)), Is.True,
            "2026-09 levy should still be in short-term, expiring at 2027-09");
        Assert.That(shortTerm[new CalendarPeriod(2027, 9)], Is.EqualTo(100m));

        // Long-term still contains September 2027 from levy 2025-09 (24 months earlier).
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2027, 9)), Is.True);
        Assert.That(longTerm[new CalendarPeriod(2027, 9)], Is.EqualTo(100m));
    }

    private static IDictionary<CalendarPeriod, decimal> BuildThreeYearFundsIn()
    {
        var fundsIn = new Dictionary<CalendarPeriod, decimal>();
        for (var year = 2019; year <= 2021; year++)
        {
            for (var month = 1; month <= 12; month++)
            {
                fundsIn[new CalendarPeriod(year, month)] = 1000m;
            }
        }
        return fundsIn;
    }

    private static IDictionary<CalendarPeriod, decimal> BuildFundsOutCoveringTwentyFourAndTwelveMonthExpiry() =>
        new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2022, 6), 1500m }
        };
}
