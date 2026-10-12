using SFA.DAS.EmployerFinance.Interfaces.OuterApi;

namespace SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Levy;

public sealed record GetLevyLastSubmissionDateRequest(long AccountId) : IGetApiRequest
{
    public string GetUrl => $"finance/levy/{AccountId}/last-declaration-date";
}