using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PeriodEndControllerTests;

public class WhenIGetPeriodEndByPeriodEndId
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
    public async Task Then_Returns_Ok_With_A_Valid_PeriodEnd_When_Data_Exists()
    {
        // Arrange
        var periodEnd = new PeriodEnd { PeriodEndId = "12345" };

        var responseModel = new Models.Payments.PeriodEnd { PeriodEndId = "12345" };

        var periodResponse = new GetPeriodEndByPeriodEndIdResponse
        {
            PeriodEnd = responseModel
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<GetPeriodEndByPeriodEndIdRequest>(req => req.PeriodEndId == periodEnd.PeriodEndId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(periodResponse);

        _mapper
            .Setup(m => m.Map<PeriodEnd>(It.IsAny<Models.Payments.PeriodEnd>()))
            .Returns<Models.Payments.PeriodEnd>(src => new PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        _mapper
            .Setup(m => m.Map<Models.Payments.PeriodEnd>(It.IsAny<PeriodEnd>()))
            .Returns<PeriodEnd>(src => new Models.Payments.PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        // Act
        var result = await _periodEndsController.GetByPeriodEndByPeriodEndId(periodEnd.PeriodEndId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var model = okResult.Value as PeriodEnd;
        model.Should().NotBeNull();
        model!.PeriodEndId.Should().Be(periodEnd.PeriodEndId);
        model.Should().BeEquivalentTo(periodEnd);
    }

}
