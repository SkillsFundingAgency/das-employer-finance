using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Data;

public class FinanceDashboardRepositoryWithCache(
    IFinanceDashboardRepository inner,
    ICacheService cacheService)
    : IFinanceDashboardRepository
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    public async Task<decimal> GetAccountBalanceAsync(long accountId)
    {
        var cacheKey = $"finance-dashboard:account-balance:{accountId}";

        var (hit, cachedValue) = await cacheService.TryGetAsync<decimal>(cacheKey);
        if (hit)
            return cachedValue;

        var result = await inner.GetAccountBalanceAsync(accountId);
        await cacheService.SetAsync(cacheKey, result, CacheExpiration);
        return result;
    }

    public async Task<decimal> GetTotalSpendForLastYearAsync(long accountId)
    {
        var cacheKey = $"finance-dashboard:total-spend-last-year:{accountId}";

        var (hit, cachedValue) = await cacheService.TryGetAsync<decimal>(cacheKey);
        if (hit)
            return cachedValue;

        var result = await inner.GetTotalSpendForLastYearAsync(accountId);
        await cacheService.SetAsync(cacheKey, result, CacheExpiration);
        return result;
    }

    public async Task<decimal> GetLastMonthPaymentsAndTransfersAsync(
        long accountId, DateTime fromDate, DateTime toDate)
    {
        var cacheKey = $"finance-dashboard:last-month-payments:{accountId}:{fromDate:yyyyMMdd}:{toDate:yyyyMMdd}";

        var (hit, cachedValue) = await cacheService.TryGetAsync<decimal>(cacheKey);
        if (hit)
            return cachedValue;

        var result = await inner.GetLastMonthPaymentsAndTransfersAsync(accountId, fromDate, toDate);
        await cacheService.SetAsync(cacheKey, result, CacheExpiration);
        return result;
    }

    public async Task<decimal> GetLatestLevyDeclarationTotalAsync(long accountId)
    {
        var cacheKey = $"finance-dashboard:latest-levy-declaration:{accountId}";

        var (hit, cachedValue) = await cacheService.TryGetAsync<decimal>(cacheKey);
        if (hit)
            return cachedValue;

        var result = await inner.GetLatestLevyDeclarationTotalAsync(accountId);
        await cacheService.SetAsync(cacheKey, result, CacheExpiration);
        return result;
    }

    public async Task<decimal> GetLevyDeclarationTotalForMonthAsync(long accountId, string payrollYear, int payrollMonth)
    {
        var cacheKey = $"finance-dashboard:levy-declaration-for-month:{accountId}:{payrollYear}:{payrollMonth}";

        var (hit, cachedValue) = await cacheService.TryGetAsync<decimal>(cacheKey);
        if (hit)
            return cachedValue;

        var result = await inner.GetLevyDeclarationTotalForMonthAsync(accountId, payrollYear, payrollMonth);
        await cacheService.SetAsync(cacheKey, result, CacheExpiration);
        return result;
    }
}
