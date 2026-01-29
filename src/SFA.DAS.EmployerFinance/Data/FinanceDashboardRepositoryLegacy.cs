using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Data;

public class FinanceDashboardRepositoryLegacy(
    ITransactionRepository transactionRepository,
    IDasLevyService levyService,
    ILogger<FinanceDashboardRepositoryLegacy> logger)
    : IFinanceDashboardRepository
{
    public async Task<decimal> GetAccountBalanceAsync(long accountId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetAccountBalanceAsync (Legacy) called for AccountId {AccountId}", accountId);

            var result = await transactionRepository.GetAccountBalance(accountId);
            
            logger.LogInformation(
                "GetAccountBalanceAsync (Legacy) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetAccountBalanceAsync (Legacy) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public async Task<decimal> GetTotalSpendForLastYearAsync(long accountId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetTotalSpendForLastYearAsync (Legacy) called for AccountId {AccountId}", accountId);

            var result = await transactionRepository.GetTotalSpendForLastYear(accountId);
            
            logger.LogInformation(
                "GetTotalSpendForLastYearAsync (Legacy) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetTotalSpendForLastYearAsync (Legacy) failed for AccountId {AccountId} after {ElapsedMs}ms",
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
                "GetLastMonthPaymentsAndTransfersAsync (Legacy) called for AccountId {AccountId}, FromDate: {FromDate}, ToDate: {ToDate}",
                accountId, fromDate, toDate);

            var transactionLines = await levyService.GetAccountTransactionsByDateRange(
                accountId, fromDate, toDate);
            
            var result = transactionLines
                .Where(c => c.TransactionType is TransactionItemType.Payment 
                    or TransactionItemType.Transfer)
                .Sum(c => c.Amount);
            
            logger.LogInformation(
                "GetLastMonthPaymentsAndTransfersAsync (Legacy) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetLastMonthPaymentsAndTransfersAsync (Legacy) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    public async Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            logger.LogInformation("GetLatestLevyDeclarationTotalAsync (Legacy) called for AccountId {AccountId}", accountId);

            var result = await levyService.GetLatestLevyDeclaration(accountId);
            
            logger.LogInformation(
                "GetLatestLevyDeclarationTotalAsync (Legacy) completed in {ElapsedMs}ms for AccountId {AccountId}, Result: {Result}",
                stopwatch.ElapsedMilliseconds, accountId, result);
            
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "GetLatestLevyDeclarationTotalAsync (Legacy) failed for AccountId {AccountId} after {ElapsedMs}ms",
                accountId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
