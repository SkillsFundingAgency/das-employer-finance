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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetAccountBalanceAsync (EF) called for AccountId {AccountId}", accountId);
            
            var result = await dbContext.Transactions
                .Where(t => t.AccountId == accountId 
                    && (t.TransactionType == TransactionItemType.Declaration
                        || t.TransactionType == TransactionItemType.TopUp
                        || t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer
                        || t.TransactionType == TransactionItemType.ExpiredFund))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            
            logger.LogInformation(
                "GetAccountBalanceAsync (EF) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetAccountBalanceAsync (EF) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public async Task<decimal> GetTotalSpendForLastYearAsync(long accountId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetTotalSpendForLastYearAsync (EF) called for AccountId {AccountId}", accountId);
            
            var oneYearAgo = DateTime.UtcNow.AddYears(-1).Date;
            var result = await dbContext.Transactions
                .Where(t => t.AccountId == accountId
                    && t.TransactionDate >= oneYearAgo
                    && (t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            
            logger.LogInformation(
                "GetTotalSpendForLastYearAsync (EF) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetTotalSpendForLastYearAsync (EF) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public async Task<decimal> GetLastMonthPaymentsAndTransfersAsync(
        long accountId, DateTime fromDate, DateTime toDate)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation(
                "GetLastMonthPaymentsAndTransfersAsync (EF) called for AccountId {AccountId}, FromDate: {FromDate}, ToDate: {ToDate}",
                accountId, fromDate, toDate);

            var result = await dbContext.Transactions
                .Where(t => t.AccountId == accountId
                    && t.DateCreated >= fromDate
                    && t.DateCreated <= toDate
                    && (t.TransactionType == TransactionItemType.Payment
                        || t.TransactionType == TransactionItemType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            
            logger.LogInformation(
                "GetLastMonthPaymentsAndTransfersAsync (EF) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetLastMonthPaymentsAndTransfersAsync (EF) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public async Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetLatestLevyDeclarationTotalAsync (EF) called for AccountId {AccountId}", accountId);

            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3).Date;

            var resultRow = await dbContext.Database
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
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var result = resultRow?.MonthlyTotal ?? 0;

            logger.LogInformation(
                "GetLatestLevyDeclarationTotalAsync (EF) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLatestLevyDeclarationTotalAsync (EF) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    private class LevyDeclarationMonthlyTotal
    {
        public decimal MonthlyTotal { get; set; }
    }
}
