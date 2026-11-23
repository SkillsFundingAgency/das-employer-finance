using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;

public sealed record GetLevyProjectionsStatusByAccountIdRequest(long AccountId) : IGetApiRequest
{
    public string GetUrl => $"finance/levy/{AccountId}/projections/status";
}
