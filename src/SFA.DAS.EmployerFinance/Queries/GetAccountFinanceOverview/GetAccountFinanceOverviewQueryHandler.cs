using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
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

        var payrollYear = query.FromDate.ToPayrollYearString();
        var payrollMonth = query.FromDate.Month >= 4 ? query.FromDate.Month - 3 : query.FromDate.Month + 9;
        var levyDeclaredForMonth = await repository.GetLevyDeclarationTotalForMonthAsync(query.AccountId, payrollYear, payrollMonth);

        var lastMonthPayments = await repository.GetLastMonthPaymentsAndTransfersAsync(
            query.AccountId, query.FromDate, query.ToDate);

        var fundsIn = levyDeclaredForMonth * 12m;

        var response = new GetAccountFinanceOverviewResponse
        {
            AccountId = query.AccountId,
            CurrentFunds = currentBalance,
            FundsIn = fundsIn,
            FundsOut = 0,
            TotalSpendForLastYear = totalSpendForLastYear,
            LastMonthLevyDeclaration = levyDeclaredForMonth,
            LastMonthPayments = lastMonthPayments
        };
        
        return response;
    }
}