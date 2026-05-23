using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetAccountBalance
{
    private const long ExpectedAccountId = 20;
    private const decimal ExpectedCurrentFunds = 2345.67M;
    private const decimal ExpectedTotalSpendForLastYear = 1000.00M;
    private const decimal TransferTotal = -50m;
    private const decimal PaymentTotal = -50m;

    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<IFinanceDashboardRepository> _repository;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;

    private DateTime _expectedToDate;
    private DateTime _expectedFromDate;

    [SetUp]
    public void Setup()
    {
        _expectedToDate = new DateTime(2000, 1, 1);
        _expectedFromDate = new DateTime(2000, 10, 1);
        _repository = new Mock<IFinanceDashboardRepository>();
        _validator = new Mock<IValidator<GetAccountFinanceOverviewQuery>>();

        _query = new GetAccountFinanceOverviewQuery { AccountId = ExpectedAccountId, FromDate = _expectedFromDate, ToDate = _expectedToDate};

        _handler = new GetAccountFinanceOverviewQueryHandler(_repository.Object, _validator.Object);

        _repository.Setup(s => s.GetAccountBalanceAsync(ExpectedAccountId)).ReturnsAsync(ExpectedCurrentFunds);
        _repository.Setup(s => s.GetTotalSpendForLastYearAsync(ExpectedAccountId)).ReturnsAsync(ExpectedTotalSpendForLastYear);
        _repository.Setup(s => s.GetLevyDeclarationTotalForMonthAsync(ExpectedAccountId, "00-01", 7)).ReturnsAsync(0);
        _repository.Setup(s => s.GetLastMonthPaymentsAndTransfersAsync(ExpectedAccountId, _expectedFromDate, _expectedToDate))
            .ReturnsAsync(PaymentTotal + TransferTotal);
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
    public async Task ThenTheFundsInShouldBeZero_WhenNoDeclarations()
    {
        _repository.Setup(s => s.GetLevyDeclarationTotalForMonthAsync(ExpectedAccountId, "00-01", 7)).ReturnsAsync(0);

        var response = await _handler.Handle(_query, CancellationToken.None);

        response.FundsIn.Should().Be(0);
    }

    [Test]
    public async Task ThenTheFundsOutShouldBeZero()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.FundsOut.Should().Be(0);
    }

    [Test]
    public async Task ThenTheTotalSpendForLastYearShouldBeReturned()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.TotalSpendForLastYear.Should().Be(ExpectedTotalSpendForLastYear);
    }

    [Test]
    public void ThenIfExceptionOccursGettingBalanceItShouldBePropagated()
    {
        var expectedException = new Exception("Test error");

        _repository.Setup(s => s.GetAccountBalanceAsync(ExpectedAccountId)).Throws(expectedException);

        Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));
    }

    [Test]
    public async Task ThenTheTotalMonthlySpendAnd()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.LastMonthPayments.Should().Be(TransferTotal + PaymentTotal);
    }
}