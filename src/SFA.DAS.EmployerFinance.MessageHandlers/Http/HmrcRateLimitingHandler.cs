using System.Net.Http;
using System.Threading;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Http;

public class HmrcRateLimitingHandler : DelegatingHandler
{
    private readonly IHmrcRateLimiter _rateLimiter;
    private readonly ILogger<HmrcRateLimitingHandler> _logger;

    public HmrcRateLimitingHandler(IHmrcRateLimiter rateLimiter, ILogger<HmrcRateLimitingHandler> logger)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public HmrcRateLimitingHandler(IHmrcRateLimiter rateLimiter, ILogger<HmrcRateLimitingHandler> logger, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var waitDuration = await _rateLimiter.WaitForAvailabilityAsync(cancellationToken);

        if (waitDuration > TimeSpan.Zero)
        {
            _logger.LogInformation("HMRC request delayed by rate limiter for {WaitDurationMs}ms", waitDuration.TotalMilliseconds);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
