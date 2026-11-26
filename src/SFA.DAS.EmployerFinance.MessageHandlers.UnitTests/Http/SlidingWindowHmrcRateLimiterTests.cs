using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.MessageHandlers.Http;

namespace SFA.DAS.EmployerFinance.MessageHandlers.UnitTests.Http;

public class SlidingWindowHmrcRateLimiterTests
{
    [Test]
    public async Task Allows_Up_To_Max_Requests_Without_Waiting()
    {
        var limiter = new SlidingWindowHmrcRateLimiter(5, TimeSpan.FromSeconds(2));

        for (var i = 0; i < 5; i++)
        {
            var wait = await limiter.WaitForAvailabilityAsync(CancellationToken.None);
            wait.Should().Be(TimeSpan.Zero);
        }
    }

    [Test]
    public async Task Delays_Sixth_Request_Until_Window_Reopens()
    {
        var window = TimeSpan.FromMilliseconds(150);
        var limiter = new SlidingWindowHmrcRateLimiter(5, window);

        for (var i = 0; i < 5; i++)
        {
            await limiter.WaitForAvailabilityAsync(CancellationToken.None);
        }

        var wait = await limiter.WaitForAvailabilityAsync(CancellationToken.None);

        wait.Should().BeGreaterThan(TimeSpan.Zero);
        wait.Should().BeLessOrEqualTo(window + TimeSpan.FromMilliseconds(50));
    }

    [Test]
    public async Task Window_Reopens_Without_Wait_After_Full_Window_Elapses()
    {
        var window = TimeSpan.FromMilliseconds(80);
        var limiter = new SlidingWindowHmrcRateLimiter(2, window);

        await limiter.WaitForAvailabilityAsync(CancellationToken.None);
        await limiter.WaitForAvailabilityAsync(CancellationToken.None);

        await Task.Delay(TimeSpan.FromMilliseconds(110));

        var wait = await limiter.WaitForAvailabilityAsync(CancellationToken.None);
        wait.Should().Be(TimeSpan.Zero);
    }

    [Test]
    public async Task Concurrent_Callers_Do_Not_Exceed_Window_Capacity()
    {
        var window = TimeSpan.FromMilliseconds(120);
        var limiter = new SlidingWindowHmrcRateLimiter(5, window);
        var grantedTimes = new ConcurrentBag<DateTimeOffset>();

        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            await limiter.WaitForAvailabilityAsync(CancellationToken.None);
            grantedTimes.Add(DateTimeOffset.UtcNow);
        });

        await Task.WhenAll(tasks);

        var ordered = grantedTimes.OrderBy(t => t).ToList();
        ordered.Should().HaveCount(10);
        (ordered[5] - ordered[0]).Should().BeGreaterOrEqualTo(TimeSpan.FromMilliseconds(95));
    }

    [Test]
    public async Task Respects_Cancellation_While_Waiting()
    {
        var limiter = new SlidingWindowHmrcRateLimiter(1, TimeSpan.FromSeconds(5));
        await limiter.WaitForAvailabilityAsync(CancellationToken.None);

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        var act = async () => await limiter.WaitForAvailabilityAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Throws_When_Max_Requests_Is_Not_Positive(int maxRequests)
    {
        var act = () => new SlidingWindowHmrcRateLimiter(maxRequests, TimeSpan.FromSeconds(2));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Throws_When_Window_Is_Not_Positive()
    {
        var act = () => new SlidingWindowHmrcRateLimiter(5, TimeSpan.Zero);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
