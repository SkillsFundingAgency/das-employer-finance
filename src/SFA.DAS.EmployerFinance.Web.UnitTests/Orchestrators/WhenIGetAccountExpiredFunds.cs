using System.Linq.Expressions;
using System.Net;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

internal class WhenIGetAccountExpiredFunds
{
    private const string HashedAccountId = "123ABC";
    private readonly DateTime _fromDate = DateTime.Now.AddDays(-30);
    private readonly DateTime _toDate = DateTime.Now.AddDays(-1);
    private readonly DateTime _transactionDate = DateTime.Now.AddDays(-5);

    private Mock<IMediator> _mediator;
    private EmployerAccountTransactionsOrchestrator _orchestrator;
    private FindEmployerAccountExpiredFundsResponse _response;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        _response = new FindEmployerAccountExpiredFundsResponse
        {
            Total = 150m,
            TwelveMonthExpiryAmount = 50m,
            TwentyFourthMonthExpiryAmount = 100m,
            TransactionDate = _transactionDate
        };

        _mediator.Setup(MediatorExpression()).ReturnsAsync(_response);

        _orchestrator = new EmployerAccountTransactionsOrchestrator(
            Mock.Of<IAccountApiClient>(),
            _mediator.Object,
            Mock.Of<ICurrentDateTime>(),
            Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(),
            Mock.Of<IEncodingService>(),
            Mock.Of<IAuthenticationOrchestrator>(),
            Mock.Of<IGovAuthEmployerAccountService>(),
            Mock.Of<EmployerFinanceWebConfiguration>());
    }

    private Expression<Func<IMediator, Task<FindEmployerAccountExpiredFundsResponse>>> MediatorExpression()
    {
        return x => x.Send(It.Is<FindEmployerAccountExpiredFundsQuery>(q =>
            q.HashedAccountId == HashedAccountId
            && q.FromDate == _fromDate
            && q.ToDate == _toDate), CancellationToken.None);
    }

    [Test]
    public async Task ThenTheMediatorIsSentWithTheCorrectQuery()
    {
        //Act
        await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        _mediator.Verify(MediatorExpression(), Times.Once);
    }

    [Test]
    public async Task ThenTheResponseStatusIsOK()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Status.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task ThenTheTotalIsMappedToTheViewModel()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Data.Total.Should().Be(_response.Total);
    }

    [Test]
    public async Task ThenTheTwelveMonthExpiryAmountIsMappedToTheViewModel()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Data.TwelveMonthExpiryAmount.Should().Be(_response.TwelveMonthExpiryAmount);
    }

    [Test]
    public async Task ThenTheTwentyFourthMonthExpiryAmountIsMappedToTheViewModel()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Data.TwentyFourthMonthExpiryAmount.Should().Be(_response.TwentyFourthMonthExpiryAmount);
    }

    [Test]
    public async Task ThenTheTransactionDateIsMappedToTheViewModel()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Data.TransactionDate.Should().Be(_transactionDate);
    }

    [Test]
    public async Task ThenTheHashedAccountIdIsSetOnTheViewModel()
    {
        //Act
        var result = await _orchestrator.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Data.HashedAccountId.Should().Be(HashedAccountId);
    }
}
