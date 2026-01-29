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
            
            var fourMonthsAgo = DateTime.UtcNow.AddMonths(-4);

            var latestPerScheme = await dbContext.Database
                .SqlQueryRaw<LevyDeclarationSummary>(
                    @"WITH LatestDeclarations AS (
                        SELECT 
                            EmpRef,
                            TotalAmount,
                            ROW_NUMBER() OVER (
                                PARTITION BY EmpRef 
                                ORDER BY PayrollYear DESC, PayrollMonth DESC
                            ) AS RowNum
                        FROM [employer_financial].[GetLevyDeclarationAndTopUp]
                        WHERE AccountId = {0}
                            AND SubmissionDate >= {1}
                            AND (LastSubmission = 1 OR EndOfYearAdjustment = 1)
                    )
                    SELECT EmpRef, TotalAmount
                    FROM LatestDeclarations
                    WHERE RowNum = 1",
                    accountId, fourMonthsAgo)
                .ToListAsync();
            
            var result = latestPerScheme.Sum(x => x.TotalAmount);
            
            logger.LogInformation(
                "GetLatestLevyDeclarationTotalAsync (EF) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}, Schemes: {SchemeCount}",
                stopwatch.ElapsedMilliseconds, accountId, result, latestPerScheme.Count);
            
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
}
