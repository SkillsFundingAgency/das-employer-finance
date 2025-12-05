using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Services.Contracts;
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
        var latestMonthly = await levyService.GetLatestLevyDeclaration(query.AccountId);

        var transactionLines = await levyService.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>
            (query.AccountId, query.FromDate, query.ToDate);

        var totalPayments = transactionLines
            .Where(c=>c.TransactionType is TransactionItemType.Payment or TransactionItemType.Transfer)
            .Sum(c => c.LineAmount);
        
        var fundsIn = latestMonthly * 12m;

        var response = new GetAccountFinanceOverviewResponse
        {
            AccountId = query.AccountId,
            CurrentFunds = currentBalance,
            FundsIn = fundsIn,
            FundsOut = 0,
            TotalSpendForLastYear = totalSpendForLastYear,
            LastMonthLevyDeclaration = latestMonthly,
            LastMonthPayments = totalPayments
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