using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Extensions;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class DasLevyService(
    ITransactionRepository transactionRepository,
    IHmrcDateService hmrcDateService,
    IDasLevyRepository dasLevyRepository,
    ICurrentDateTime currentDateTime)
    : IDasLevyService
{
    public Task<decimal> GetAccountBalance(long accountId)
    {
        return transactionRepository.GetAccountBalance(accountId);
    }

    public Task<TransactionLine[]> GetAccountTransactionsByDateRange(long accountId, DateTime fromDate, DateTime toDate)
    {
        return transactionRepository.GetAccountTransactionsByDateRange(accountId, fromDate, toDate);
    }

    public async Task<T[]> GetAccountProviderPaymentsByDateRange<T>(long accountId, long ukprn, DateTime fromDate,
        DateTime toDate) where T : TransactionLine
    {
        var transactions = await transactionRepository.GetAccountTransactionByProviderAndDateRange(
            accountId,
            ukprn,
            fromDate,
            toDate);

        var result = transactions.OfType<T>().ToArray();

        EnsureAllPayrollDatesAreSet(result);

        return result;
    }

    public async Task<T[]> GetAccountCoursePaymentsByDateRange<T>(long accountId, long ukprn, string courseName,
        int? courseLevel, int? pathwayCode, DateTime fromDate,
        DateTime toDate) where T : TransactionLine
    {
        var transactions = await transactionRepository.GetAccountCoursePaymentsByDateRange(
            accountId,
            ukprn,
            courseName,
            courseLevel,
            pathwayCode,
            fromDate,
            toDate);

        var result = transactions.OfType<T>().ToArray();

        EnsureAllPayrollDatesAreSet(result);

        return result;
    }

    public Task<string> GetProviderName(long ukprn, long accountId, string periodEnd)
    {
        return transactionRepository.GetProviderName(ukprn, accountId, periodEnd);
    }

    public Task<int> GetPreviousAccountTransaction(long accountId, DateTime fromDate)
    {
        return transactionRepository.GetPreviousTransactionsCount(accountId, fromDate);
    }

    public async Task<T[]> GetAccountLevyTransactionsByDateRange<T>(long accountId, DateTime fromDate,
        DateTime toDate) where T : TransactionLine
    {
        var transactions = await transactionRepository.GetAccountLevyTransactionsByDateRange(
            accountId,
            fromDate,
            toDate);

        var result = transactions.OfType<T>().ToArray();

        EnsureAllPayrollDatesAreSet(result);

        return result;
    }

    private void EnsureAllPayrollDatesAreSet<T>(IEnumerable<T> transactions) where T : TransactionLine
    {
        foreach (var transaction in transactions)
        {
            if (!string.IsNullOrEmpty(transaction.PayrollYear) && transaction.PayrollMonth != 0)
            {
                transaction.PayrollDate =
                    hmrcDateService.GetDateFromPayrollYearMonth(transaction.PayrollYear, transaction.PayrollMonth);
            }
        }
    }

    public Task<decimal> GetTotalSpendForLastYear(long accountId)
    {
        return transactionRepository.GetTotalSpendForLastYear(accountId);
    }

    public async Task<decimal> GetLatestLevyDeclaration(long accountId)
    {
        var declarations = await dasLevyRepository.GetAccountLevyDeclarations(accountId);
        if (declarations == null)
        {
            return 0;
        }

        var forecastDeclarations = GetForecastDeclarations(declarations);
        var fundsInMonthly = forecastDeclarations.Sum(fd => fd.TotalAmount);
        return fundsInMonthly;
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
                .Where(x => IsRecentDeclaration(x, currentPayrollYear, currentPayrollMonth, previousPayrollYear))
                .OrderByDescending(x => x.PayrollYear)
                .ThenByDescending(x => x.PayrollMonth)
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