using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

[TestFixture]
internal class WhenIGetLevySummaryByAccountId
{
    private FinanceLevyDeclarationsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<LevyDeclarationOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();

        var orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, _logger.Object);

        _controller = new FinanceLevyDeclarationsController(orchestrator);
    }

    [Test, MoqAutoData]
    public async Task ThenReturnTheSummaryReturned(long accountId)
    {
        //Arrange
        var accountBalancesResponse = new GetLevySummaryByAccountIdQueryResult
        {
            Summary = new LevySummary { CurrentLevyFunds = 10 }
        };

        _mediator.Setup(x => x.Send(It.Is<GetLevySummaryByAccountIdQuery>(q => q.AccountId == accountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountBalancesResponse);

        //Act
        var response = await _controller.GetLevySummary(accountId);

        //Assert
        response.Should().NotBeNull();
    }
}