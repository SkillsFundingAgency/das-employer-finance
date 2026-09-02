using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyControllerTests;

[TestFixture]
internal class WhenIGetLevySummaryByAccountId
{
    private FinanceLevyController _controller;
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

        _controller = new FinanceLevyController(orchestrator);
    }

    [Test]
    public async Task ThenReturnTheSummaryReturned()
    {
        //Arrange
        const string hashedAccountId = "ABC1234";

        var accountBalancesResponse = new GetLevySummaryQueryResult
        {
            Summary = new LevySummary { CurrentLevyFunds = 10 }
        };

        _mediator.Setup(x => x.Send(It.Is<GetLevySummaryQuery>(q => q.HashedAccountId == hashedAccountId), It.IsAny<CancellationToken>())).ReturnsAsync(accountBalancesResponse);

        //Act
        var response = await _controller.GetLevySummary(hashedAccountId);

        //Assert
        response.Should().NotBeNull();
    }
}