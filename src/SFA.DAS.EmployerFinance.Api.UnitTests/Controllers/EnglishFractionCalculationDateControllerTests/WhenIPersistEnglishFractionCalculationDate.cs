using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.EnglishFractionCalculationDateControllerTests;

public class WhenIPersistEnglishFractionCalculationDate
{
    private EnglishFractionCalculationDateController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<EnglishFractionCalculationDateOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<EnglishFractionCalculationDateOrchestrator>>();

        var orchestrator = new EnglishFractionCalculationDateOrchestrator(_mediator.Object, _logger.Object);
        _controller = new EnglishFractionCalculationDateController(orchestrator);
    }

    [Test]
    public async Task ThenItReturnsOk_WhenRequestIsValid()
    {
        var request = new EnglishFractionCalculationDateRequest
        {
            DateCalculated = new DateTime(2025, 01, 01)
        };

        var result = await _controller.Persist(request);

        result.Should().BeOfType<OkResult>();

        _mediator.Verify(x =>
            x.Send(It.Is<CreateEnglishFractionCalculationDateCommand>(c =>
                c.DateCalculated == request.DateCalculated), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ThenItReturnsBadRequest_WhenRequestIsNull()
    {
        var result = await _controller.Persist(null);

        result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)result).Value.Should().Be("Request payload is required.");
    }

    [Test]
    public async Task ThenItReturnsBadRequestWithValidationErrors_WhenDateIsInvalid()
    {
        var request = new EnglishFractionCalculationDateRequest
        {
            DateCalculated = DateTime.MinValue
        };

        var validationResult = new System.ComponentModel.DataAnnotations.ValidationResult(
            "Validation failed",
            new[]
            {
                "DateCalculated|DateCalculated has not been supplied"
            });

        _mediator
            .Setup(x => x.Send(It.IsAny<CreateEnglishFractionCalculationDateCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(validationResult, null, null));

        var result = await _controller.Persist(request);

        result.Should().BeOfType<BadRequestObjectResult>();

        var response = ((BadRequestObjectResult)result).Value as Dictionary<string, string>;
        response.Should().NotBeNull();
        response!.Should().ContainKey("DateCalculated");
        response["DateCalculated"].Should().Be("DateCalculated has not been supplied");
    }
}
