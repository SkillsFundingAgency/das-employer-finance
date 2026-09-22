using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(
    IOuterApiClient outerApiClient,
    IInProcessCache cache) : IOuterApiService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static string LevySummaryKey(long accountId) => $"LevySummary_{accountId}";

    public async Task<GetLevySummaryByAccountIdResponse> GetLevySummary(long accountId, bool refreshCache = false)
    {
        var key = LevySummaryKey(accountId);

        if (!refreshCache && cache.Exists(key))
            return cache.Get<GetLevySummaryByAccountIdResponse>(key);

        var response = await outerApiClient.Get<GetLevySummaryByAccountIdResponse>(
            new GetLevySummaryByAccountIdRequest(accountId));

        cache.Set(key, response, CacheDuration);

        return response;
    }
}