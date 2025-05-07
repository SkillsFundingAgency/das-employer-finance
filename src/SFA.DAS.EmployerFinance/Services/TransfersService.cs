using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Transfers;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Transfers;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class TransfersService : ITransfersService
{
    private readonly IOuterApiClient _outerApiClient;

    public TransfersService(
        IOuterApiClient apiClient)
    {
        _outerApiClient = apiClient;
    }

    public async Task<GetCountsResponse> GetCounts(long accountId)
    {
        return await _outerApiClient.Get<GetCountsResponse>(new GetCountsRequest(accountId));
    }
}