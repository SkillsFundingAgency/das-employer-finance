using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(IOuterApiClient outerApiClient,
    IInProcessCache cache,
    IEnumerable<ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>> invalidationRules,
    ILogger<OuterApiService> logger) : IOuterApiService
{
    private static readonly TimeSpan CacheDuration30Days = TimeSpan.FromDays(30);
    private static string LevyProjectionsKey(long accountId, int months) => $"LevyProjections_{accountId}_{months}";

    public async Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId,
        int months = 6,
        bool refreshCache = false,
        CancellationToken cancellationToken = default)
    {
        var key = LevyProjectionsKey(accountId, months);

        if (!refreshCache && cache.Exists(key))
        {
            var cached = cache.Get<GetLevyProjectionsByAccountIdResponse>(key);
            var context = new CacheInvalidationContext(accountId);
            var shouldInvalidate = await invalidationRules.AnyAsync(r => r.ShouldInvalidateAsync(cached, context));
            if (!shouldInvalidate)
                return cached;
        }

        try
        {
            var response = await outerApiClient.Get<GetLevyProjectionsByAccountIdResponse>(new GetLevyProjectionsByAccountIdRequest(accountId, months), cancellationToken);
            response.LastRefreshDateTime = DateTime.UtcNow; // Set the last refresh time to now since we just fetched fresh data
            cache.Set(key, response, CacheDuration30Days);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve levy projections for AccountId={AccountId}, Months={Months}. " +
                "Returning default response to avoid blocking the UI.",
                accountId, months);

            if (!cache.Exists(key)) return new GetLevyProjectionsByAccountIdResponse(); // safe empty default

            logger.LogWarning("Serving stale cached levy projections for AccountId={AccountId}.", accountId);
            return cache.Get<GetLevyProjectionsByAccountIdResponse>(key);
        }
    }
}