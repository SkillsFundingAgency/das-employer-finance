using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Queries.GetAccountTransactionSummary;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests;

[TestFixture]
public class WhenIGetTheTransactionSummaryForAnAccount
{
    private AccountTransactionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AccountTransactionsOrchestrator>> _logger;
    private Mock<ILinkGeneratorWrapper> _linkGenerator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AccountTransactionsOrchestrator>>();
        _linkGenerator = new Mock<ILinkGeneratorWrapper>();
        var orchestrator = new AccountTransactionsOrchestrator(_mediator.Object, _logger.Object, _linkGenerator.Object);
        _controller = new AccountTransactionsController(orchestrator, _linkGenerator.Object);
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

        _linkGenerator.Setup(
                x => x.GetPathByName(
                    "GetTransactions", 
                    It.Is<object>(o => o.IsEquivalentTo(new 
                    { 
                        hashedAccountId, 
                        year = transactionSummaryResponse.Data.First().Year, 
                        month = transactionSummaryResponse.Data.First().Month 
                    }))))
            .Returns(firstExpectedUri);

        var secondExpectedUri = "someotheruri";

        _linkGenerator.Setup(
                x => x.GetPathByName(
                    "GetTransactions",
                    It.Is<object>(o => o.IsEquivalentTo(new
                    { 
                        hashedAccountId, 
                        year = transactionSummaryResponse.Data.Last().Year, 
                        month = transactionSummaryResponse.Data.Last().Month 
                    }))))
            .Returns(secondExpectedUri);

        //Act
        var response = await _controller.Index(hashedAccountId);

        //Assert            
        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        var model = ((OkObjectResult)response).Value as List<TransactionSummary>;

        model?.Should().NotBeNull();
        model?.Should().BeEquivalentTo(transactionSummaryResponse.Data, x => x.Excluding(y => y.Href));
        model?.First().Href.Should().Be(firstExpectedUri);
        model?.Last().Href.Should().Be(secondExpectedUri);
    }      
}