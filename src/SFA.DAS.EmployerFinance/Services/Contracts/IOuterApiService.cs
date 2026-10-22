using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IOuterApiService
{
    Task<GetLevyProjectionsByAccountIdResponse> GetLevyProjections(long accountId, int months = 6, bool refreshCache = false, CancellationToken cancellationToken = default);
}