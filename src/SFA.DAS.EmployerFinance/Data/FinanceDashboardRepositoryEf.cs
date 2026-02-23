using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.Data;

public class FinanceDashboardRepositoryEf(
    EmployerFinanceDbContext dbContext,
    ILogger<FinanceDashboardRepositoryEf> logger)
    : IFinanceDashboardRepository
{
    public async Task<decimal> GetAccountBalanceAsync(long accountId)
    {
        try
        {
            return await dbContext.Transactions
                .Where(t => t.AccountId == accountId
                    && (t.TransactionType == TransactionItemType.Declaration
                        || t.TransactionType == TransactionItemType.TopUp
                        || t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer
                        || t.TransactionType == TransactionItemType.ExpiredFund))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAccountBalanceAsync (EF) failed for AccountId {AccountId}", accountId);
            throw;
        }
    }

    public async Task<decimal> GetTotalSpendForLastYearAsync(long accountId)
    {
        try
        {
            var oneYearAgo = DateTime.UtcNow.AddYears(-1).Date;
            return await dbContext.Transactions
                .Where(t => t.AccountId == accountId
                    && t.TransactionDate >= oneYearAgo
                    && (t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetTotalSpendForLastYearAsync (EF) failed for AccountId {AccountId}", accountId);
            throw;
        }
    }

    public async Task<decimal> GetLastMonthPaymentsAndTransfersAsync(
        long accountId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            return await dbContext.Transactions
                .Where(t => t.AccountId == accountId
                    && t.DateCreated >= fromDate
                    && t.DateCreated <= toDate
                    && (t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLastMonthPaymentsAndTransfersAsync (EF) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }

    public async Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId)
    {
        try
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3).Date;

            var resultRows = await dbContext.Database
                .SqlQueryRaw<LevyDeclarationMonthlyTotal>(
                    @"WITH LatestPerPaye AS (
                        SELECT ld.AccountId, ld.EmpRef, ld.SubmissionDate, ld.PayrollYear, ld.PayrollMonth, ld.SubmissionId,
                            ROW_NUMBER() OVER (PARTITION BY ld.EmpRef ORDER BY ld.PayrollYear DESC, ld.PayrollMonth DESC) AS rn
                        FROM [employer_financial].[LevyDeclaration] ld
                        WHERE ld.AccountId = {0}
                    )
                    SELECT ISNULL(SUM(CASE WHEN l.SubmissionDate >= {1} THEN ISNULL(v.TotalAmount, 0) ELSE 0 END), 0) AS MonthlyTotal
                    FROM LatestPerPaye l
                    LEFT JOIN [employer_financial].[GetLevyDeclarationAndTopUp] v
                        ON v.AccountId = l.AccountId AND v.EmpRef = l.EmpRef AND v.PayrollYear = l.PayrollYear AND v.PayrollMonth = l.PayrollMonth AND v.SubmissionId = l.SubmissionId
                    WHERE l.rn = 1",
                    accountId, threeMonthsAgo)
                .ToListAsync();

            return resultRows.FirstOrDefault()?.MonthlyTotal ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLatestLevyDeclarationTotalAsync (EF) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }

    public async Task<decimal> GetLevyDeclarationTotalForMonthAsync(long accountId, string payrollYear, int payrollMonth)
    {
        try
        {
            var resultRows = await dbContext.Database
                .SqlQueryRaw<LevyDeclarationMonthlyTotal>(
                    @"SELECT ISNULL(SUM(TotalAmount), 0) AS MonthlyTotal
                      FROM [employer_financial].[GetLevyDeclarationAndTopUp]
                      WHERE AccountId = {0} AND PayrollYear = {1} AND PayrollMonth = {2}
                        AND (LastSubmission = 1 OR EndOfYearAdjustment = 1)",
                    accountId, payrollYear, payrollMonth)
                .ToListAsync();

            return resultRows.FirstOrDefault()?.MonthlyTotal ?? 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLevyDeclarationTotalForMonthAsync (EF) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }

    private class LevyDeclarationMonthlyTotal
    {
        public decimal MonthlyTotal { get; set; }
    }
}
