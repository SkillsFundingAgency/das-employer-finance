using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IOuterApiService
{
    Task<GetLevySummaryByHashedAccountIdResponse> GetLevySummary(string hashedAccountId, bool refreshCache = false);
}