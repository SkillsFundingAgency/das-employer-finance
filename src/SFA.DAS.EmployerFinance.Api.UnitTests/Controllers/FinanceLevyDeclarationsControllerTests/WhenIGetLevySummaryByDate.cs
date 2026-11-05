using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

[TestFixture]
internal class WhenIGetLevySummaryByDate
{
    private FinanceLevyDeclarationsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<LevyDeclarationOrchestrator>> _logger;
    private LevyDeclarationOrchestrator _orchestrator;
    private Mock<IMapper> _mapper;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<LevyDeclarationOrchestrator>>();
        _mapper = new Mock<IMapper>();
        _orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, _mapper.Object, _logger.Object);
        _controller = new FinanceLevyDeclarationsController(_orchestrator);
    }

    [Test, MoqAutoData]
    public async Task ThenReturnTheSummaryReturned(long accountId,
        DateOnly fromDate,
        DateOnly toDate,
        GetLevyDeclarationsByAccountAndDateRangeQueryResult queryResult,
        List<LevyDeclaration> mappedDeclarations)
    {
        // Arrange
        _mediator
            .Setup(x => x.Send(
                It.Is<GetLevyDeclarationsByAccountAndDateRangeQuery>(q =>
                    q.AccountId == accountId &&
                    q.FromDate == fromDate &&
                    q.ToDate == toDate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(queryResult);

        // Mapper is called once per item in response._
        for (var i = 0; i < queryResult._.Count; i++)
        {
            var item = queryResult._[i];
            _mapper
                .Setup(x => x.Map<LevyDeclaration>(item))
                .Returns(mappedDeclarations[i]);
        }

        // Act
        var result = await _controller.GetLevyDeclarationSummaryByDate(accountId, fromDate, toDate) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeAssignableTo<IEnumerable<LevyDeclaration>>()
            .Which.Should().BeEquivalentTo(mappedDeclarations);
    }
}