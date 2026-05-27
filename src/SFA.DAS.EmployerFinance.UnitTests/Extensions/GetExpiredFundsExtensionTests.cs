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
            expired: new Dictionary<CalendarPeriod, decimal>(),
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
            expired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // Levy 2019-01 to 2021-01 (≤ policyChangeDate) produces expiry dates 2021-01 to 2023-01.
        // All 25 are ≤ today (2023-06).
        Assert.That(longTerm.Count, Is.EqualTo(25));
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 1)), Is.True, "Earliest 24-month expiry should be present");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2023, 1)), Is.True, "Latest 24-month expiry should be present");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2023, 2)), Is.False, "Post-policyChangeDate levy should not appear in LongTerm");

        // Payment at 2022-06 = 1500: covers 2020-06 levy fully (500 left), then partially covers 2020-07 levy.
        Assert.That(longTerm[new CalendarPeriod(2022, 6)], Is.EqualTo(0m));
        Assert.That(longTerm[new CalendarPeriod(2022, 7)], Is.EqualTo(500m));
        Assert.That(longTerm.Values.Sum(), Is.EqualTo(23500m));
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
            expired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // Levy 2021-02 to 2021-12 (> policyChangeDate) produces 12-month expiry dates 2022-02 to 2022-12.
        // All 11 are ≤ today (2023-06).
        Assert.That(shortTerm.Count, Is.EqualTo(11));
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 2)), Is.True, "Earliest 12-month expiry should be present");
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 12)), Is.True, "Latest 12-month expiry should be present");
        Assert.That(shortTerm.ContainsKey(new CalendarPeriod(2022, 1)), Is.False, "Levy on the policyChangeDate itself (2021-01) should not appear in ShortTerm");

        Assert.That(shortTerm.Values.Sum(), Is.EqualTo(11000m));
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
            expired: new Dictionary<CalendarPeriod, decimal>(),
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
            expired: new Dictionary<CalendarPeriod, decimal>(),
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        // With today = 2022-06 only expiry dates ≤ 2022-06 are included.
        // LongTerm: levy 2019-01 to 2020-06 → expiry 2021-01 to 2022-06 (18 entries).
        Assert.That(longTerm.Count, Is.EqualTo(18));
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2022, 6)), Is.True);
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2022, 7)), Is.False,
            "Expiry dates beyond today should be excluded");

        // ShortTerm: levy 2021-02 to 2021-06 → expiry 2022-02 to 2022-06 (5 entries).
        Assert.That(shortTerm.Count, Is.EqualTo(5));
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
            expired: existingExpired,
            expiryPeriod: 24,
            today: today,
            policyChangeDate: policyChangeDate);

        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 1)), Is.False,
            "Already-expired 2021-01 should be excluded from new results");
        Assert.That(longTerm.ContainsKey(new CalendarPeriod(2021, 2)), Is.False,
            "Already-expired 2021-02 should be excluded from new results");

        // Remaining 23 periods should still be calculated.
        Assert.That(longTerm.Count, Is.EqualTo(23));
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
