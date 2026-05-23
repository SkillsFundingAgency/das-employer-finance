using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetExpiringFunds
{
    private const long ExpectedAccountId = 20;
    private const long ExpectedBalance = 2000;
    private const decimal ExpectedTotalSpendForLastYear = 1000.00M;

    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<IFinanceDashboardRepository> _repository;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<IFinanceDashboardRepository>();
        _validator = new Mock<IValidator<GetAccountFinanceOverviewQuery>>();

        _query = new GetAccountFinanceOverviewQuery { AccountId = ExpectedAccountId };

        _handler = new GetAccountFinanceOverviewQueryHandler(_repository.Object, _validator.Object);

        _repository.Setup(s => s.GetAccountBalanceAsync(ExpectedAccountId)).ReturnsAsync(ExpectedBalance);
        _repository.Setup(s => s.GetTotalSpendForLastYearAsync(ExpectedAccountId)).ReturnsAsync(ExpectedTotalSpendForLastYear);
        _repository.Setup(s => s.GetLevyDeclarationTotalForMonthAsync(ExpectedAccountId, It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(0);
        _repository.Setup(s => s.GetLastMonthPaymentsAndTransfersAsync(ExpectedAccountId, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0m);
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