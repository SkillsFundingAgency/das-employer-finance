using System.Threading;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Http;

public interface IHmrcRateLimiter
{
    Task<TimeSpan> WaitForAvailabilityAsync(CancellationToken cancellationToken);
}
