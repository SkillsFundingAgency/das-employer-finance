using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevySummary;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EmployerAccountsControllerTests;

[TestFixture]
internal class WhenIGetLevySummaryByAccountId
{
    private EmployerAccountsController _employerAccountsController;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<FinanceOrchestrator>> _logger;
    private Mock<IMapper> _mapper;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<FinanceOrchestrator>>();
        _mapper = new Mock<IMapper>();
        _encodingService = new Mock<IEncodingService>();

        var orchestrator = new FinanceOrchestrator(_mediator.Object, _logger.Object, _mapper.Object, _encodingService.Object);

        _employerAccountsController = new EmployerAccountsController(orchestrator);
    }

    [Test]
    public async Task ThenReturnTheSummaryReturned()
    {
        //Arrange
        const string hashedAccountId = "ABC1234";

        var accountBalancesResponse = new GetLevySummaryResponse
        {
            Summary = new LevySummary { CurrentLevyFunds = 10 }
        };

        _mediator.Setup(x => x.Send(It.Is<GetLevySummaryQuery>(q => q.HashedAccountId == hashedAccountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountBalancesResponse);

        //Act
        var response = await _employerAccountsController.GetLevySummary(hashedAccountId);

        //Assert
        response.Should().NotBeNull();
    }
}