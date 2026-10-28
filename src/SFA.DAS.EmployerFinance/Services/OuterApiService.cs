using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(IOuterApiClient outerApiClient) : IOuterApiService
{
    public async Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId,
        int months = 6,
        CancellationToken cancellationToken = default)
    {
            var response = await outerApiClient.Get<GetLevyProjectionsByAccountIdResponse>(new GetLevyProjectionsByAccountIdRequest(accountId, months), cancellationToken);
            response.LastRefreshDateTime = DateTime.UtcNow; // Set the last refresh time to now since we just fetched fresh data
            return response;
    }
}