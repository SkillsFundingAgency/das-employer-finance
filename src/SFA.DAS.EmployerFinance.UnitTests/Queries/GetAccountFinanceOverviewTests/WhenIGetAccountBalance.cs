using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetAccountFinanceOverviewTests;

public class WhenIGetAccountBalance
{
    private const long ExpectedAccountId = 20;
    private const decimal ExpectedCurrentFunds = 2345.67M;
    private const decimal ExpectedTotalSpendForLastYear = 1000.00M;
    private const decimal TransferTotal = 50m;
    private const decimal PaymentTotal = 50m;

    private GetAccountFinanceOverviewQueryHandler _handler;
    private Mock<ILogger<GetAccountFinanceOverviewQueryHandler>> _logger;
    private Mock<IDasLevyService> _levyService;
    private Mock<IValidator<GetAccountFinanceOverviewQuery>> _validator;
    private GetAccountFinanceOverviewQuery _query;

    private DateTime _expectedToDate;
    private DateTime _expectedFromDate;

    [SetUp]
    public void Setup()
    {
        _expectedToDate = new DateTime(2000, 1, 1);
        _expectedFromDate = new DateTime(2000, 10, 1);
        _logger = new Mock<ILogger<GetAccountFinanceOverviewQueryHandler>>();
        _levyService = new Mock<IDasLevyService>();
        _validator = new Mock<IValidator<GetAccountFinanceOverviewQuery>>();

        _query = new GetAccountFinanceOverviewQuery { AccountId = ExpectedAccountId, FromDate = _expectedFromDate, ToDate = _expectedToDate};

        _handler = new GetAccountFinanceOverviewQueryHandler(_levyService.Object, _validator.Object, _logger.Object);
        
        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).ReturnsAsync(ExpectedCurrentFunds);
        _levyService.Setup(s => s.GetTotalSpendForLastYear(ExpectedAccountId)).ReturnsAsync(ExpectedTotalSpendForLastYear);
        _levyService.Setup(s => s.GetLatestLevyDeclaration(ExpectedAccountId)).ReturnsAsync(0);
        _levyService.Setup(s =>
                s.GetAccountLevyTransactionsByDateRange<LevyDeclarationTransactionLine>(ExpectedAccountId,
                    _expectedFromDate, _expectedToDate))
            .ReturnsAsync([
                new LevyDeclarationTransactionLine
                {
                    TransactionType = TransactionItemType.Payment,
                    LineAmount = PaymentTotal
                },
                new LevyDeclarationTransactionLine
                {
                    TransactionType = TransactionItemType.Transfer,
                    LineAmount = TransferTotal
                },
                new LevyDeclarationTransactionLine
                {
                    TransactionType = TransactionItemType.Declaration,
                    LineAmount = 100m
                }
            ]);
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
        _levyService.Setup(s => s.GetLatestLevyDeclaration(ExpectedAccountId)).ReturnsAsync(0);

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
    public void ThenIfExceptionOccursGettingBalanceItShouldBeLogged()
    {
        var expectedException = new Exception("Test error");

        _levyService.Setup(s => s.GetAccountBalance(ExpectedAccountId)).Throws(expectedException);

        Assert.ThrowsAsync<Exception>(() => _handler.Handle(_query, CancellationToken.None));

        _logger.VerifyLogging($"Failed to get account's current balance for account ID: {_query.AccountId}", LogLevel.Error, Times.Once());
    }
    
    [Test]
    public async Task ThenTheTotalMonthlySpendAnd()
    {
        var response = await _handler.Handle(_query, CancellationToken.None);

        response.LastMonthPayments.Should().Be(TransferTotal + PaymentTotal);
    }
}