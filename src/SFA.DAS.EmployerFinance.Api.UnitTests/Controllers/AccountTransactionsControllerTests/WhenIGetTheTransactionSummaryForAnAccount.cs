using Microsoft.AspNetCore.Mvc.Routing;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests;

[TestFixture]
public class WhenIGetTheTransactionSummaryForAnAccount
{
    private AccountTransactionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AccountTransactionsOrchestrator>> _logger;
    private Mock<IUrlHelper> _urlHelper;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AccountTransactionsOrchestrator>>();
        _urlHelper = new Mock<IUrlHelper>();
        var orchestrator = new AccountTransactionsOrchestrator(_mediator.Object, _logger.Object);
        _controller = new AccountTransactionsController(orchestrator, _urlHelper.Object);
        _controller.Url = _urlHelper.Object;
    }

    [Test]
    public async Task ThenTheTransactionSummaryIsReturned()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var transactionSummaryResponse = new GetAccountTransactionSummaryResponse
        {
            Data = new List<TransactionSummary> { new TransactionSummary { Month = 1, Year = 2017 }, new TransactionSummary { Month = 2, Year = 2017 } }
        };
        _mediator.Setup(x => x.Send(It.Is<GetAccountTransactionSummaryRequest>(q => q.HashedAccountId == hashedAccountId),It.IsAny<CancellationToken>())).ReturnsAsync(transactionSummaryResponse);
             

        var firstExpectedUri = "someuri";
        _urlHelper.Setup(
                x =>
                    x.RouteUrl(
                        It.Is<UrlRouteContext>(c =>
                            c.RouteName == "GetTransactions" && c.Values.IsEquivalentTo(new { hashedAccountId, year = transactionSummaryResponse.Data.First().Year, month = transactionSummaryResponse.Data.First().Month }))))
            .Returns(firstExpectedUri);

        var secondExpectedUri = "someotheruri";

        _urlHelper.Setup(
                x =>
                    x.RouteUrl(
                        It.Is<UrlRouteContext>(c =>
                            c.RouteName== "GetTransactions" && c.Values.IsEquivalentTo(new { hashedAccountId, year = transactionSummaryResponse.Data.Last().Year, month = transactionSummaryResponse.Data.Last().Month }))))
            .Returns(secondExpectedUri);

        //Act
        var response = await _controller.Index(hashedAccountId);

        //Assert            
        Assert.IsNotNull(response);
        Assert.IsInstanceOf<OkObjectResult>(response);
        var model = ((OkObjectResult)response).Value as List<TransactionSummary>;

        model?.Should().NotBeNull();
        model?.ShouldAllBeEquivalentTo(transactionSummaryResponse.Data, x => x.Excluding(y => y.Href));
        model?.First().Href.Should().Be(firstExpectedUri);
        model?.Last().Href.Should().Be(secondExpectedUri);
    }      
}