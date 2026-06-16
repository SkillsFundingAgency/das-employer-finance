using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EnglishFractionsControllerTests;

public class WhenIPersistEnglishFractions
{
    private EnglishFractionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<EnglishFractionsOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EnglishFractionsOrchestrator>>();

        var orchestrator = new EnglishFractionsOrchestrator(_mediator.Object, _logger.Object);
        _controller = new EnglishFractionsController(orchestrator);
    }

    [Test]
    public async Task ThenItReturnsOkWithStoredAndIgnoredCounts_WhenUpdateIsRequired()
    {
        // Arrange
        var request = new EnglishFractionsRequest
        {
            EmpRef = "123/AB456",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<EnglishFractionItem>
            {
                new() { DateCalculated = new DateTime(2024, 12, 01), Amount = 0.5m },
                new() { DateCalculated = new DateTime(2025, 01, 01), Amount = 0.6m }
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<PersistEnglishFractionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersistEnglishFractionsResponse { Stored = 2, Ignored = 0 });

        // Act
        var result = await _controller.Persist(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var response = result.Value as EnglishFractionsResponse;
        response.Should().NotBeNull();
        response!.Stored.Should().Be(2);
        response.Ignored.Should().Be(0);
    }

    [Test]
    public async Task ThenItReturnsOkWithIgnoredCount_WhenNoUpdateIsRequired()
    {
        // Arrange
        var request = new EnglishFractionsRequest
        {
            EmpRef = "123/AB456",
            UpdateRequired = false,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<EnglishFractionItem>
            {
                new() { DateCalculated = new DateTime(2024, 12, 01), Amount = 0.5m }
            }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<PersistEnglishFractionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersistEnglishFractionsResponse { Stored = 0, Ignored = 1 });

        // Act
        var result = await _controller.Persist(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var response = result.Value as EnglishFractionsResponse;
        response.Should().NotBeNull();
        response!.Stored.Should().Be(0);
        response.Ignored.Should().Be(1);
    }

    [Test]
    public async Task ThenItReturnsBadRequest_WhenRequestIsNull()
    {
        // Act
        var result = await _controller.Persist(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)result).Value.Should().Be("Request payload is required.");
    }

    [Test]
    public async Task ThenItReturnsBadRequestWithValidationErrors_WhenFractionsAreEmpty()
    {
        var request = new EnglishFractionsRequest
        {
            EmpRef = "123/AB456",
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<EnglishFractionItem>()
        };

        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            new[]
            {
                "Fractions|Fractions payload is required."
            });

        _mediator
            .Setup(x => x.Send(It.IsAny<PersistEnglishFractionsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var result = await _controller.Persist(request);

        result.Should().BeOfType<BadRequestObjectResult>();

        var response = ((BadRequestObjectResult)result).Value as Dictionary<string, string>;
        response.Should().NotBeNull();
        response!.Should().ContainKey("Fractions");
        response["Fractions"].Should().Be("Fractions payload is required.");
    }

    [Test]
    public async Task ThenItReturnsBadRequestWithValidationErrors_WhenValidationFails()
    {
        var request = new EnglishFractionsRequest
        {
            EmpRef = string.Empty,
            UpdateRequired = true,
            DateCalculated = new DateTime(2025, 01, 01),
            Fractions = new List<EnglishFractionItem>
            {
                new() { DateCalculated = new DateTime(2025, 01, 01), Amount = 0.6m }
            }
        };

        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            new[]
            {
                "EmployerReference|EmployerReference has not been supplied"
            });

        _mediator
            .Setup(x => x.Send(It.IsAny<PersistEnglishFractionsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var result = await _controller.Persist(request);

        result.Should().BeOfType<BadRequestObjectResult>();

        var response = ((BadRequestObjectResult)result).Value as Dictionary<string, string>;
        response.Should().NotBeNull();
        response!.Should().ContainKey("EmployerReference");
        response["EmployerReference"].Should().Be("EmployerReference has not been supplied");
    }
}
