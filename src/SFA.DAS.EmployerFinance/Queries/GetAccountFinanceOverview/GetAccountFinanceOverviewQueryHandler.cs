using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQueryHandler(
    IFinanceDashboardRepository repository,
    IValidator<GetAccountFinanceOverviewQuery> validator)
    : IRequestHandler<GetAccountFinanceOverviewQuery, GetAccountFinanceOverviewResponse>
{
    public async Task<GetAccountFinanceOverviewResponse> Handle(GetAccountFinanceOverviewQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(query);
        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var currentBalance = await repository.GetAccountBalanceAsync(query.AccountId);
        var totalSpendForLastYear = await repository.GetTotalSpendForLastYearAsync(query.AccountId);
        var latestMonthly = await repository.GetLatestLevyDeclarationTotalAsync(query.AccountId);
        var lastMonthPayments = await repository.GetLastMonthPaymentsAndTransfersAsync(
            query.AccountId, query.FromDate, query.ToDate);
        
        var fundsIn = latestMonthly * 12m;

        var response = new GetAccountFinanceOverviewResponse
        {
            AccountId = query.AccountId,
            CurrentFunds = currentBalance,
            FundsIn = fundsIn,
            FundsOut = 0,
            TotalSpendForLastYear = totalSpendForLastYear,
            LastMonthLevyDeclaration = latestMonthly,
            LastMonthPayments = lastMonthPayments
        };
        
        return response;
    }
}