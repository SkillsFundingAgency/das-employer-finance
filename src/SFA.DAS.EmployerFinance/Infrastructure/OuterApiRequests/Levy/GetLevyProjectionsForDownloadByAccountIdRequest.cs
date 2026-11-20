using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;

public sealed record GetLevyProjectionsForDownloadByAccountIdRequest(long AccountId) : IGetApiRequest
{
    public string GetUrl => $"finance/levy/{AccountId}/projections/download";
}
