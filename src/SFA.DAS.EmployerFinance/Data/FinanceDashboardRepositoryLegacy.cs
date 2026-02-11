using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.Data;

public class FinanceDashboardRepositoryLegacy(
    ITransactionRepository transactionRepository,
    IDasLevyService levyService,
    IDasLevyRepository dasLevyRepository,
    ILogger<FinanceDashboardRepositoryLegacy> logger)
    : IFinanceDashboardRepository
{
    public async Task<decimal> GetAccountBalanceAsync(long accountId)
    {
        try
        {
            return await transactionRepository.GetAccountBalance(accountId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetAccountBalanceAsync (Legacy) failed for AccountId {AccountId}", accountId);
            throw;
        }
    }

    public async Task<decimal> GetTotalSpendForLastYearAsync(long accountId)
    {
        try
        {
            return await transactionRepository.GetTotalSpendForLastYear(accountId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GetTotalSpendForLastYearAsync (Legacy) failed for AccountId {AccountId}", accountId);
            throw;
        }
    }

    public async Task<decimal> GetLastMonthPaymentsAndTransfersAsync(
        long accountId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var transactionLines = await levyService.GetAccountTransactionsByDateRange(
                accountId, fromDate, toDate);

            return transactionLines
                .Where(c => c.TransactionType is TransactionItemType.Payment
                    or TransactionItemType.Transfer)
                .Sum(c => c.Amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLastMonthPaymentsAndTransfersAsync (Legacy) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }

    public async Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId)
    {
        try
        {
            return await levyService.GetLatestLevyDeclaration(accountId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLatestLevyDeclarationTotalAsync (Legacy) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }

    public async Task<decimal> GetLevyDeclarationTotalForMonthAsync(long accountId, string payrollYear, int payrollMonth)
    {
        try
        {
            var declarations = await dasLevyRepository.GetAccountLevyDeclarations(accountId, payrollYear, (short)payrollMonth);
            return declarations.Sum(d => d.TotalAmount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "GetLevyDeclarationTotalForMonthAsync (Legacy) failed for AccountId {AccountId}",
                accountId);
            throw;
        }
    }
}
