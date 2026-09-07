using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IOuterApiService
{
    Task<GetLevySummaryByAccountIdResponse> GetLevySummary(long accountId, bool refreshCache = false);
}