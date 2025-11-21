using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PeriodEndControllerTests;

[TestFixture]
public class WhenIGetAllPeriodEnds
{
    private PeriodEndController _periodEndsController;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<PeriodEndOrchestrator>> _logger;
    private Mock<IMapper> _mapper;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<PeriodEndOrchestrator>>();
        _mapper = new Mock<IMapper>();

        var orchestrator = new PeriodEndOrchestrator(_mediator.Object, _logger.Object, _mapper.Object);
        _periodEndsController = new PeriodEndController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_With_List_Of_PeriodEnds_When_Data_Exists()
    {
        // Arrange
        var periodEndModels = new List<Models.Payments.PeriodEnd>
    {
        new() { PeriodEndId = "12345" },
        new() { PeriodEndId = "45652" }
    };

        var periodResponse = new GetPeriodEndsResponse
        {
            CurrentPeriodEnds = periodEndModels
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetPeriodEndsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(periodResponse);

        _mapper
            .Setup(m => m.Map<PeriodEnd>(It.IsAny<Models.Payments.PeriodEnd>()))
            .Returns<Models.Payments.PeriodEnd>(x => new PeriodEnd { PeriodEndId = x.PeriodEndId });

        // Act
        var result = await _periodEndsController.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        var model = ((OkObjectResult)result).Value as List<PeriodEnd>;
        model.Should().NotBeNull();
        model.Count.Should().Be(2);
        model[0].PeriodEndId.Should().Be("12345");
        model[1].PeriodEndId.Should().Be("45652");
    }

    [Test]
    public async Task Then_Returns_NotFound_When_No_Data_Exists()
    {
        // Arrange
        _mediator
            .Setup(x => x.Send(It.IsAny<GetPeriodEndsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPeriodEndsResponse)null);

        // Act
        var result = await _periodEndsController.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<NotFoundResult>();
    }
}
