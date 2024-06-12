using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiRequests.Projections;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Projections;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Interfaces.OuterApi;
using SFA.DAS.EmployerFinance.Models.ExpiringFunds;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQueryHandler : IRequestHandler<GetAccountFinanceOverviewQuery, GetAccountFinanceOverviewResponse>
{
    private readonly ICurrentDateTime _currentDateTime;
    private readonly IDasForecastingService _dasForecastingService;
    private readonly IDasLevyService _levyService;
    private readonly IOuterApiClient _outerApiClient;
    private readonly IValidator<GetAccountFinanceOverviewQuery> _validator;
    private readonly ILogger<GetAccountFinanceOverviewQueryHandler> _logger;

    public GetAccountFinanceOverviewQueryHandler(
        ICurrentDateTime currentDateTime,
        IDasForecastingService dasForecastingService,
        IDasLevyService levyService,
        IOuterApiClient outerApiClient,
        IValidator<GetAccountFinanceOverviewQuery> validator,
        ILogger<GetAccountFinanceOverviewQueryHandler> logger)
    {
        _currentDateTime = currentDateTime;
        _dasForecastingService = dasForecastingService;
        _levyService = levyService;
        _outerApiClient = outerApiClient;
        _validator = validator;
        _logger = logger;
    }

    public async Task<GetAccountFinanceOverviewResponse> Handle(GetAccountFinanceOverviewQuery query, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query);
        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var currentBalance = await GetAccountBalance(query.AccountId);

        var accountProjectionSummaryFromFinance = await _outerApiClient.Get<GetAccountProjectionSummaryFromFinanceResponse>(
            new GetAccountProjectionSummaryFromFinanceRequest(query.AccountId));

        var accountProjectionSummary = await _dasForecastingService.GetAccountProjectionSummary(query.AccountId);
        var earliestFundsToExpire = GetExpiringFunds(accountProjectionSummary?.ExpiringAccountFunds);
        var projectedCalculations = accountProjectionSummary?.ProjectionCalulation;
        var totalSpendForLastYear = await GetTotalSpendForLastYear(query.AccountId);

        var response = new GetAccountFinanceOverviewResponse
        {
            AccountId = query.AccountId,
            CurrentFunds = currentBalance,
            FundsIn = accountProjectionSummaryFromFinance?.FundsIn ?? 0,
            FundsOut = projectedCalculations?.FundsOut ?? 0,
            TotalSpendForLastYear = totalSpendForLastYear
        };

        if (earliestFundsToExpire != null)
        {
            response.ExpiringFundsExpiryDate = earliestFundsToExpire.PayrollDate;
            response.ExpiringFundsAmount = earliestFundsToExpire.Amount;
        }

        return response;
    }

    private ExpiringFunds GetExpiringFunds(ExpiringAccountFunds expiringFunds)
    {
        var today = _currentDateTime.Now.Date;
        var nextYear = today.AddDays(1 - today.Day).AddMonths(13);
        var earliestFundsToExpire = expiringFunds?.ExpiryAmounts?
            .Where(a => a.PayrollDate < nextYear && a.PayrollDate > today)
            .OrderBy(a => a.PayrollDate)
            .FirstOrDefault();

        return earliestFundsToExpire;
    }

    private async Task<decimal> GetAccountBalance(long accountId)
    {
        try
        {
            _logger.LogInformation($"Getting current funds balance for account ID: {accountId}");

            return await _levyService.GetAccountBalance(accountId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to get account's current balance for account ID: {accountId}");

            throw;
        }
    }

    private Task<decimal> GetTotalSpendForLastYear(long accountId)
    {
        return _levyService.GetTotalSpendForLastYear(accountId);
    }
}