using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;

public sealed record GetLevyProjectionsByAccountIdRequest(long AccountId, int Months = 12) : IGetApiRequest
{
    public string GetUrl => $"finance/levy/{AccountId}/projections?months={Months}";
}