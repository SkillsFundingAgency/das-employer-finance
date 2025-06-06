﻿using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ProjectedCalculations;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetAccountBalance
{
    private const long ExpectedAccountId = 20;
    private const decimal ExpectedCurrentFunds = 2345.67M;
    private const decimal ExpectedFundsIn = 1234.56M;
    private const decimal ExpectedFundsOut = 789.01M;

    private DateTime _now;
    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<IDasForecastingService> _forecastingService;
    private Mock<ILogger<GetAccountFinanceOverviewQueryHandler>> _logger;
    private Mock<IDasLevyService> _levyService;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;
    private AccountProjectionSummary _accountProjectionSummary;

    [SetUp]
    public void Setup()
    {
        _now = DateTime.UtcNow;
        _logger = new Mock<ILogger<GetAccountFinanceOverviewQueryHandler>>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _forecastingService = new Mock<IDasForecastingService>();
        _levyService = new Mock<IDasLevyService>();
        _validator = new Mock<IValidator<GetAccountFinanceOverviewQuery>>();

        _query = new GetAccountFinanceOverviewQuery { AccountId = ExpectedAccountId };

        _accountProjectionSummary = new AccountProjectionSummary
        {
            AccountId = ExpectedAccountId,
            ProjectionGenerationDate = DateTime.UtcNow,
            ProjectionCalulation = new ProjectedCalculation
            {
                FundsIn = ExpectedFundsIn,
                FundsOut = ExpectedFundsOut,
                NumberOfMonths = 12
            }
        };

        _handler = new GetAccountFinanceOverviewQueryHandler(_forecastingService.Object,
            _levyService.Object, _validator.Object,
            _logger.Object);
        _currentDateTime.Setup(d => d.Now).Returns(_now);
        _forecastingService.Setup(s => s.GetAccountProjectionSummary(ExpectedAccountId)).ReturnsAsync(_accountProjectionSummary);
        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).ReturnsAsync(ExpectedCurrentFunds);
        _validator.Setup(v => v.ValidateAsync(_query))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
    }

    [Test]
    public async Task ThenTheCurrentFundsShouldBeReturned()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.CurrentFunds.Should().Be(ExpectedCurrentFunds);
    }

    [Test]
    public async Task ThenTheFundsInShouldBeReturned()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.FundsIn.Should().Be(ExpectedFundsIn);
    }

    [Test]
    public async Task ThenTheFundsOutShouldBeReturned()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.FundsOut.Should().Be(ExpectedFundsOut);
    }

    [Test]
    public async Task ThenZeroFundsShouldBeReturnedIfNull()
    {
        _forecastingService.Setup(s => s.GetAccountProjectionSummary(ExpectedAccountId)).ReturnsAsync(new AccountProjectionSummary());

        var response = await _handler.Handle(_query, CancellationToken.None);

        response.FundsIn.Should().Be(0);
        response.FundsOut.Should().Be(0);
    }

    [Test]
    public void ThenIfExceptionOccursGettingBalanceItShouldBeLogged()
    {
        var expectedException = new Exception("Test error");

        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).Throws(expectedException);

        Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));

        _logger.VerifyLogging($"Failed to get account's current balance for account ID: {_query.AccountId}", LogLevel.Error, Times.Once());
    }
}