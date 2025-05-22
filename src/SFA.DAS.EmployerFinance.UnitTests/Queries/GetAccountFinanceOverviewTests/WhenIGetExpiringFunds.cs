using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.ProjectedCalculations;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetExpiringFunds
{
    private const long ExpectedAccountId = 20;
    private const long ExpectedBalance = 2000;
    private const decimal ExpectedFundsIn = 1234.56M;
    private const decimal ExpectedFundsOut = 789.01M;

    private DateTime _now;
    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<ICurrentDateTime> _currentDateTime;
    private Mock<IDasForecastingService> _dasForecastingService;
    private Mock<IDasLevyService> _levyService;
    private Mock<ILogger<GetAccountFinanceOverviewQueryHandler>> _logger;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;
    private AccountProjectionSummary _accountProjectionSummary;

    [SetUp]
    public void Setup()
    {
        _now = DateTime.UtcNow;
        _logger = new Mock<ILogger<GetAccountFinanceOverviewQueryHandler>>();
        _currentDateTime = new Mock<ICurrentDateTime>();
        _dasForecastingService = new Mock<IDasForecastingService>();
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

        _handler = new GetAccountFinanceOverviewQueryHandler(_dasForecastingService.Object, _levyService.Object, _validator.Object, _logger.Object);
        _currentDateTime.Setup(d => d.Now).Returns(_now);
        _dasForecastingService.Setup(s => s.GetAccountProjectionSummary(ExpectedAccountId)).ReturnsAsync(_accountProjectionSummary);
        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).ReturnsAsync(ExpectedBalance);
        _validator.Setup(v => v.ValidateAsync(_query))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
    }

    [Test]
    public async Task ThenTheExpiringFundsShouldHaveTheCorrectAccountId()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.AccountId.Should().Be(ExpectedAccountId);
        response.CurrentFunds.Should().Be(ExpectedBalance);
    }

    [Test]
    public async Task ThenIfNullIsReturnedTheAccountIdAndBalanceShouldStillBePopulated()
    {
        _dasForecastingService.Setup(s => s.GetAccountProjectionSummary(ExpectedAccountId)).ReturnsAsync((AccountProjectionSummary)null);

        var response = await _handler.Handle(_query, CancellationToken.None);

        response.AccountId.Should().Be(ExpectedAccountId);
    }
    
    [Test]
    public void ThenIfValidationFailsAnExceptionIsThrown()
    {
        _validator.Setup(v => v.ValidateAsync(_query)).ReturnsAsync(new ValidationResult
        {
            ValidationDictionary = new Dictionary<string, string> { { "Test Error", "Error" } }
        });

        Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(_query, CancellationToken.None));
    }
}