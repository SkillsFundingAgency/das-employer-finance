using AutoMapper;
using Microsoft.AspNetCore.Http;
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
    public async Task Then_Returns_Created_With_Newly_Created_Period_End()
    {
        // Arrange
        var inputPeriodEnd = new PeriodEnd { PeriodEndId = "12345" };

        var mappedPeriodEnd = new Models.Payments.PeriodEnd { PeriodEndId = "12345" };

        _mapper
            .Setup(m => m.Map<Models.Payments.PeriodEnd>(It.IsAny<PeriodEnd>()))
            .Returns<PeriodEnd>(src => new Models.Payments.PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        _mapper
            .Setup(m => m.Map<PeriodEnd>(It.IsAny<Models.Payments.PeriodEnd>()))
            .Returns<Models.Payments.PeriodEnd>(src => new PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        _mediator
            .Setup(x => x.Send(It.IsAny<CreateNewPeriodEndCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _periodEndsController.Create(inputPeriodEnd);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedAtActionResult>();

        var createdResult = result as CreatedAtActionResult;
        createdResult!.StatusCode.Should().Be(201);

        var model = createdResult.Value as PeriodEnd;
        model.Should().NotBeNull();
        model!.PeriodEndId.Should().Be(inputPeriodEnd.PeriodEndId);
    }

    [Test]
    public async Task Create_WhenRequestIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _periodEndsController.Create(null);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        var badRequestResult = result as BadRequestResult;
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task Create_WhenCreatedPeriodEndIsNull_ReturnsInternalServerError()
    {
        // Arrange
        var inputPeriodEnd = new PeriodEnd { PeriodEndId = "12345" };

        _mapper
            .Setup(m => m.Map<Models.Payments.PeriodEnd>(It.IsAny<PeriodEnd>()))
            .Returns<PeriodEnd>(src => new Models.Payments.PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        _mapper
            .Setup(m => m.Map<PeriodEnd>(It.IsAny<Models.Payments.PeriodEnd>()))
            .Returns<Models.Payments.PeriodEnd>(src => new PeriodEnd
            {
                PeriodEndId = src.PeriodEndId
            });

        _mediator
            .Setup(x => x.Send(It.IsAny<CreateNewPeriodEndCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mediator
            .Setup(x => x.Send(It.Is<GetPeriodEndByPeriodEndIdRequest>(r => r.PeriodEndId == "12345"), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPeriodEndByPeriodEndIdResponse)null);

        // Act
        var result = await _periodEndsController.Create(inputPeriodEnd);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Could not create period end");
    }

}
