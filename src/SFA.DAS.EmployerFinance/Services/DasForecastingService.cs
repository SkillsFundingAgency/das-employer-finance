using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Projections;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Projections;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.ProjectedCalculations;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Services;

public class DasForecastingService(IOuterApiClient apiClient, ILogger<DasForecastingService> logger)
    : IDasForecastingService
{
    public async Task<AccountProjectionSummary> GetAccountProjectionSummary(long accountId)
    {
        AccountProjectionSummary accountProjectionSummary = null;

        try
        {
            logger.LogInformation("Getting forecasting projection summary for account ID: {AccountId}", accountId);

            var accountProjectionSummaryResponse = await apiClient.Get<GetAccountProjectionSummaryResponse>(new GetAccountProjectionSummaryRequest(accountId));

            if (accountProjectionSummaryResponse != null)
            {
                accountProjectionSummary = MapFrom(accountProjectionSummaryResponse);
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Could not find forecasting projection summary for account ID: {AccountId} when calling forecast API", accountId);
        }

        return accountProjectionSummary;
    }

    private static AccountProjectionSummary MapFrom(GetAccountProjectionSummaryResponse accountProjectionSummaryResponse)
    {
        return new AccountProjectionSummary
        {
            AccountId = accountProjectionSummaryResponse.AccountId,
            ProjectionGenerationDate = accountProjectionSummaryResponse.ProjectionGenerationDate,
            ProjectionCalulation = new ProjectedCalculation
            {
                FundsIn = accountProjectionSummaryResponse.FundsIn,
                FundsOut = accountProjectionSummaryResponse.FundsOut,
                NumberOfMonths = accountProjectionSummaryResponse.NumberOfMonths
            }
        };
    }
}