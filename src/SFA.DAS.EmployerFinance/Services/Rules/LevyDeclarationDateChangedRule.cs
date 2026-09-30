using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models;

namespace SFA.DAS.EmployerFinance.Services.Rules;

public class LevyDeclarationDateChangedRule(IOuterApiClient outerApiClient)
    : ICacheInvalidationRule<GetLevyProjectionsByAccountIdResponse>
{
    public async Task<bool> ShouldInvalidateAsync(
        GetLevyProjectionsByAccountIdResponse cached,
        CacheInvalidationContext context)
    {
        var response = await outerApiClient.Get<GetLevyLastSubmissionDateResponse>(
            new GetLevyLastSubmissionDateRequest(context.AccountId));

        return cached.LastRefreshDateTime != response.LatestLevyDeclarationInDate;
    }
}