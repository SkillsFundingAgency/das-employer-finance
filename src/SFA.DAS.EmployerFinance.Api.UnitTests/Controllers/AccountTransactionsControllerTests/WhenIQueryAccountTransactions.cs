using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests;
[TestFixture]
public class WhenIQueryAccountTransactions
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
    public async Task Then_Have_InValid_Data_Returns_BadRequestResult()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        //Act
        var response = await _controller.QueryAccountTransactions(hashedAccountId, null, null);

        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<BadRequestResult>();
    }

    [Test]
    public async Task Then_Have_Valid_Data_Returns_Transactions()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var fromDate = new DateTime(2024, 12, 31);
        var toDate = new DateTime(2025, 12, 31);

        var transactionsResponse = new GetEmployerAccountTransactionsResponse
        {
            Data = TransactionLineObjectMother.Create(),
            AccountHasPreviousTransactions = false
        };

        _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>
                (q => q.HashedAccountId == hashedAccountId
                && q.FromDate == fromDate
                && q.ToDate == toDate),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionsResponse)
            .Verifiable();

        //Act
        var response = await _controller.QueryAccountTransactions(hashedAccountId, fromDate, toDate);

        //Assert
        _mediator.Verify();

        response.Should().NotBeNull();
        response.Should().BeOfType<OkObjectResult>();
        var model = ((OkObjectResult)response).Value as Transactions;

        model?.Should().NotBeNull();
        model?.PreviousMonthUri.Should().BeNullOrEmpty();
    }
}
