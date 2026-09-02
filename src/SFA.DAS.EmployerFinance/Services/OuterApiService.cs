using SFA.DAS.Caches;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(IOuterApiClient outerApiClient, IInProcessCache cache) : IOuterApiService
{
    private const string LevySummaryKey = "LevySummary";

    public async Task<GetLevySummaryByHashedAccountIdResponse> GetLevySummary(string hashedAccountId, bool refreshCache = false)
    {
        if (!cache.Exists(LevySummaryKey) || refreshCache)
        {
            var response = await outerApiClient.Get<GetLevySummaryByHashedAccountIdResponse>(new GetLevySummaryByHashedAccountIdRequest(hashedAccountId));

            cache.Set(LevySummaryKey, response);
        }

        return cache.Get<GetLevySummaryByHashedAccountIdResponse>(LevySummaryKey);        
    }
}