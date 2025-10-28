using AutoMapper;
using Moq;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PeriodEndControllerTests;

public class CreatePeriodEnd
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
    public async Task Then_Returns_Ok_With_No_Error_And_Newly_Created_Period_End()
    {
        // Arrange
        var periodEnd = new PeriodEnd() { PeriodEndId = "12345" };

        _mediator
            .Setup(x => x.Send(It.IsAny<CreateNewPeriodEndCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        // Act
        var result = await _periodEndsController.Create(periodEnd);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var model = ((OkObjectResult)result).Value as List<PeriodEnd>;
        model.Should().NotBeNull();
    }
}
