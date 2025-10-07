using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQueryHandler(
    IDasLevyService levyService,
    IValidator<GetAccountFinanceOverviewQuery> validator,
    ILogger<GetAccountFinanceOverviewQueryHandler> logger)
    : IRequestHandler<GetAccountFinanceOverviewQuery, GetAccountFinanceOverviewResponse>
{
    public async Task<GetAccountFinanceOverviewResponse> Handle(GetAccountFinanceOverviewQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query);
        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var currentBalance = await GetAccountBalance(query.AccountId);
        var totalSpendForLastYear = await GetTotalSpendForLastYear(query.AccountId);

        var response = new GetAccountFinanceOverviewResponse
        {
            AccountId = query.AccountId,
            CurrentFunds = currentBalance,
            FundsIn = 0,
            FundsOut = 0,
            TotalSpendForLastYear = totalSpendForLastYear
        };
        
        return response;
    }
    
    private async Task<decimal> GetAccountBalance(long accountId)
    {
        try
        {
            logger.LogInformation("Getting current funds balance for account ID: {AccountId}", accountId);

            return await levyService.GetAccountBalance(accountId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to get account's current balance for account ID: {AccountId}", accountId);

            throw;
        }
    }

    private Task<decimal> GetTotalSpendForLastYear(long accountId)
    {
        return levyService.GetTotalSpendForLastYear(accountId);
    }
}