using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class OuterApiService(IOuterApiClient outerApiClient) : IOuterApiService
{
    public Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId,
        int months = 6,
        bool triggerRefresh = true,
        CancellationToken cancellationToken = default)
    {
        return outerApiClient.Get<GetLevyProjectionsByAccountIdResponse>(new GetLevyProjectionsByAccountIdRequest(accountId, months, triggerRefresh), cancellationToken);
    }

    public Task<GetLevyProjectionsStatusByAccountIdResponse> GetLevyProjectionsStatus(long accountId,
        CancellationToken cancellationToken = default)
    {
        return outerApiClient.Get<GetLevyProjectionsStatusByAccountIdResponse>(new GetLevyProjectionsStatusByAccountIdRequest(accountId), cancellationToken);
    }

    public Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjectionsForDownload(long accountId,
        CancellationToken cancellationToken = default)
    {
        return outerApiClient.Get<GetLevyProjectionsByAccountIdResponse>(new GetLevyProjectionsForDownloadByAccountIdRequest(accountId), cancellationToken);
    }
}