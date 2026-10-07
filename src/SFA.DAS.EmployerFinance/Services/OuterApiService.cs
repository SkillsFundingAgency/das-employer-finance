using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(
    IOuterApiClient outerApiClient,
    IInProcessCache cache,
    IEnumerable<ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>> invalidationRules) : IOuterApiService
{
    private static readonly TimeSpan CacheDuration24Hrs = TimeSpan.FromHours(24);
    private static readonly TimeSpan CacheDuration30Days = TimeSpan.FromDays(30);
    private static string LevySummaryKey(long accountId) => $"LevySummary_{accountId}";
    private static string LevyProjectionsKey(long accountId, int months) => $"LevyProjections_{accountId}_{months}";

    public async Task<GetLevySummaryByAccountIdResponse> GetLevySummary(long accountId, bool refreshCache = false)
    {
        var key = LevySummaryKey(accountId);

        if (!refreshCache && cache.Exists(key))
            return cache.Get<GetLevySummaryByAccountIdResponse>(key);

        var response = await outerApiClient.Get<GetLevySummaryByAccountIdResponse>(
            new GetLevySummaryByAccountIdRequest(accountId));

        cache.Set(key, response, CacheDuration24Hrs);

        return response;
    }

    public async Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId, int months = 6, bool refreshCache = false)
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

        var response = await outerApiClient.Get<GetLevyProjectionsByAccountIdResponse>(
            new GetLevyProjectionsByAccountIdRequest(accountId, months));

        cache.Set(key, response, CacheDuration30Days);
        response.LastRefreshDateTime = DateTime.UtcNow; // Set the last refresh time to now since we just fetched fresh data
        return response;
    }
}