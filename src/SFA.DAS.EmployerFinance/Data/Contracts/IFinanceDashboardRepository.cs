namespace SFA.DAS.EmployerFinance.Data.Contracts;

public interface IFinanceDashboardRepository
{
    Task<decimal> GetAccountBalanceAsync(long accountId);
    Task<decimal> GetTotalSpendForLastYearAsync(long accountId);
    Task<decimal> GetLastMonthPaymentsAndTransfersAsync(long accountId, DateTime fromDate, DateTime toDate);
    Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId);
}
