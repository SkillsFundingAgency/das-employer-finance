using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IOuterApiService
{
    Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId,
        int months = 6,
        bool triggerRefresh = true,
        CancellationToken cancellationToken = default);

    Task<GetLevyProjectionsStatusByAccountIdResponse> GetLevyProjectionsStatus(long accountId,
        CancellationToken cancellationToken = default);

    Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjectionsForDownload(long accountId,
        CancellationToken cancellationToken = default);
}