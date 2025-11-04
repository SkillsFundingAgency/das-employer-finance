using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetExpiringFunds
{
    private const long ExpectedAccountId = 20;
    private const long ExpectedBalance = 2000;
    private const decimal ExpectedTotalSpendForLastYear = 1000.00M;

    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<IDasLevyService> _levyService;
    private Mock<ILogger<GetAccountFinanceOverviewQueryHandler>> _logger;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<GetAccountFinanceOverviewQueryHandler>>();
        _levyService = new Mock<IDasLevyService>();
        _validator = new Mock<IValidator<GetAccountFinanceOverviewQuery>>();

        _query = new GetAccountFinanceOverviewQuery { AccountId = ExpectedAccountId };

        _handler = new GetAccountFinanceOverviewQueryHandler(_levyService.Object, _validator.Object, _logger.Object);
        
        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).ReturnsAsync(ExpectedBalance);
        _levyService.Setup(s => s.GetTotalSpendForLastYear(ExpectedAccountId)).ReturnsAsync(ExpectedTotalSpendForLastYear);
        _levyService.Setup(s => s.GetLatestLevyDeclaration(ExpectedAccountId)).ReturnsAsync(0);
        _validator.Setup(v => v.ValidateAsync(_query))
            .ReturnsAsync(new ValidationResult { ValidationDictionary = new Dictionary<string, string>() });
    }

    [Test]
    public async Task ThenTheResponseShouldHaveTheCorrectAccountId()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.AccountId.Should().Be(ExpectedAccountId);
        response.CurrentFunds.Should().Be(ExpectedBalance);
        response.FundsIn.Should().Be(0);
        response.FundsOut.Should().Be(0);
        response.TotalSpendForLastYear.Should().Be(ExpectedTotalSpendForLastYear);
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