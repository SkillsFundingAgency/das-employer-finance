using SFA.DAS.EmployerFinance.Models.ProjectedCalculations;

namespace SFA.DAS.EmployerFinance.Services.Contracts;

public interface IDasForecastingService
{
    Task<AccountProjectionSummary> GetAccountProjectionSummary(long accountId);
}