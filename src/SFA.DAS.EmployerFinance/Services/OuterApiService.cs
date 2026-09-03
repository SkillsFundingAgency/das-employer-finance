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
    private static string LevySummaryKey(string hashedAccountId) => $"LevySummary_{hashedAccountId}";

    public async Task<GetLevySummaryByHashedAccountIdResponse> GetLevySummary(string hashedAccountId, bool refreshCache = false)
    {
        var key = LevySummaryKey(hashedAccountId);

        if (!refreshCache && cache.Exists(key))
            return cache.Get<GetLevySummaryByHashedAccountIdResponse>(key);

        var response = await outerApiClient.Get<GetLevySummaryByHashedAccountIdResponse>(
            new GetLevySummaryByHashedAccountIdRequest(hashedAccountId));

        cache.Set(key, response);
        return response;
    }
}