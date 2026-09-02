using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;

public sealed record GetLevySummaryByHashedAccountIdRequest(string HashedAccountId) : IGetApiRequest
{
    public string GetUrl => $"/finance/levy/{HashedAccountId}/summary";
}