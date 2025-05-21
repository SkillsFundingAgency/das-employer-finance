using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountProjectionSummary;

public class GetAccountProjectionSummaryHandler(
    IDasLevyRepository repository,
    ICurrentDateTime currentDateTime,
    ILogger<GetAccountProjectionSummaryHandler> logger)
    : IRequestHandler<GetAccountProjectionSummaryQuery, GetAccountProjectionSummaryResult>
{
    public const int PreviousMonths = 12;
    
    public async Task<GetAccountProjectionSummaryResult> Handle(GetAccountProjectionSummaryQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("GettingAccountProjectionSummary for accountId {Id}", query.AccountId);

        var declarations = await repository.GetAccountLevyDeclarationsForPreviousMonths(query.AccountId, PreviousMonths);
        var forecastDeclarations = GetForecastDeclarations(declarations);

        var fundsIn = forecastDeclarations.Sum(fd => fd.TotalAmount) * 12;

        return new GetAccountProjectionSummaryResult
        {
            AccountId = query.AccountId,
            FundsIn = fundsIn
        };
    }

    private List<LevyDeclarationItem> GetForecastDeclarations(IEnumerable<LevyDeclarationItem> declarations)
    {
        var groupedDeclarations = declarations.GroupBy(x => x.EmpRef).ToList();
        var (currentPayrollYear, currentPayrollMonth) = GetPayrollYearAndMonthForLastMonth(currentDateTime.Now);
        var previousPayrollYear = currentDateTime.Now.AddYears(-1).ToPayrollYearString();

        var forecastDeclarations = new List<LevyDeclarationItem>();

        foreach (var group in groupedDeclarations)
        {
            var forecastDeclaration = group
                .Where(levyDeclarationItem => IsRecentDeclaration(levyDeclarationItem, currentPayrollYear, currentPayrollMonth, previousPayrollYear))
                .OrderByDescending(levyDeclarationItem => levyDeclarationItem.PayrollYear)
                .ThenByDescending(levyDeclarationItem => levyDeclarationItem.PayrollMonth)
                .Take(2)
                .FirstOrDefault();

            if (forecastDeclaration != null)
            {
                forecastDeclarations.Add(forecastDeclaration);
            }
        }

        return forecastDeclarations;
    }

    private static bool IsRecentDeclaration(LevyDeclarationItem declaration, string currentPayrollYear, int currentPayrollMonth, string previousPayrollYear)
    {
        return ((declaration.PayrollYear == currentPayrollYear && declaration.PayrollMonth >= currentPayrollMonth - 3)
               || (declaration.PayrollYear == previousPayrollYear && declaration.PayrollMonth > 9))
               && declaration.TotalAmount > 0;
    }

    private static (string, int) GetPayrollYearAndMonthForLastMonth(DateTime currentDate)
    {
        var currentMonth = currentDate.Month;
        var payrollMonth = currentMonth >= 4 ? currentMonth - 3 : currentMonth + 9;

        return (currentDate.ToPayrollYearString(), payrollMonth);
    }
}