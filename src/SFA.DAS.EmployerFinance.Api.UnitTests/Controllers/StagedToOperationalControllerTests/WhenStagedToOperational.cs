using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.StagedToOperationalControllerTests;

[TestFixture]
public class WhenStagedToOperational
{
    private StagedToOperationalController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _controller = new StagedToOperationalController(_mediator.Object);
    }

    [Test]
    public async Task StagedToOperational_WhenRequestIsNull_ReturnsBadRequest()
    {
        var result = await _controller.StagedToOperational(null);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequest.Value.Should().Be("Transfer staged-to-operational payload is required.");
    }

    [Test]
    public async Task StagedToOperational_WhenValidationErrorsExist_ReturnsBadRequest()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<TransferStagedToOperationalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferStagedToOperationalResponse
            {
                HasValidationErrors = true,
                ValidationErrors = ["AccountId must be greater than 0"]
            });

        var result = await _controller.StagedToOperational(new TransferStagedToOperationalRequest
        {
            AccountId = 0,
            PeriodEnd = "2526-R01"
        });

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeEquivalentTo(new[] { "AccountId must be greater than 0" });
    }

    [Test]
    public async Task StagedToOperational_WhenProcessingFails_ReturnsInternalServerError()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<TransferStagedToOperationalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferStagedToOperationalResponse
            {
                IsSuccess = false,
                Message = "An unexpected error occurred while transferring staged data to operational."
            });

        var result = await _controller.StagedToOperational(new TransferStagedToOperationalRequest
        {
            AccountId = 123,
            PeriodEnd = "2526-R01",
            CorrelationId = "corr-1"
        });

        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Test]
    public async Task StagedToOperational_WhenSuccessful_ReturnsCreatedWithBody()
    {
        _mediator
            .Setup(x => x.Send(
                It.Is<TransferStagedToOperationalCommand>(command =>
                    command.AccountId == 123 &&
                    command.PeriodEnd == "2526-R01" &&
                    command.CorrelationId == "corr-1"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferStagedToOperationalResponse
            {
                IsSuccess = true,
                ProcessedCount = 7,
                Message = "Successfully transferred 7 staged rows to operational."
            });

        var result = await _controller.StagedToOperational(new TransferStagedToOperationalRequest
        {
            AccountId = 123,
            PeriodEnd = "2526-R01",
            CorrelationId = "corr-1"
        });

        result.Should().BeOfType<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);

        var json = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
        using var document = System.Text.Json.JsonDocument.Parse(json);
        document.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("processedCount").GetInt32().Should().Be(7);
        document.RootElement.GetProperty("message").GetString()
            .Should().Be("Successfully transferred 7 staged rows to operational.");
    }
}
