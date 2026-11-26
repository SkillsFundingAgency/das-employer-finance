using System.Threading;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Http;

public class SlidingWindowHmrcRateLimiter : IHmrcRateLimiter
{
    private static readonly TimeSpan MinimumDelay = TimeSpan.FromMilliseconds(1);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Queue<DateTimeOffset> _requestTimestamps = new();
    private readonly int _maxRequestsPerWindow;
    private readonly TimeSpan _window;
    private readonly TimeProvider _timeProvider;

    public SlidingWindowHmrcRateLimiter(int maxRequestsPerWindow, TimeSpan window, TimeProvider timeProvider = null)
    {
        if (maxRequestsPerWindow <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRequestsPerWindow), maxRequestsPerWindow, "Maximum requests per window must be greater than zero.");
        }

        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window), window, "Rate-limit window must be greater than zero.");
        }

        _maxRequestsPerWindow = maxRequestsPerWindow;
        _window = window;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<TimeSpan> WaitForAvailabilityAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset? waitStartedAt = null;

        while (true)
        {
            TimeSpan waitFor;

            await _gate.WaitAsync(cancellationToken);
            try
            {
                var now = _timeProvider.GetUtcNow();
                while (_requestTimestamps.Count > 0 && now - _requestTimestamps.Peek() >= _window)
                {
                    _requestTimestamps.Dequeue();
                }

                if (_requestTimestamps.Count < _maxRequestsPerWindow)
                {
                    _requestTimestamps.Enqueue(now);
                    return waitStartedAt.HasValue ? now - waitStartedAt.Value : TimeSpan.Zero;
                }

                waitStartedAt ??= now;
                waitFor = _requestTimestamps.Peek() + _window - now;
            }
            finally
            {
                _gate.Release();
            }

            var delay = waitFor < MinimumDelay ? MinimumDelay : waitFor;
            await Task.Delay(delay, _timeProvider, cancellationToken);
        }
    }
}
