using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class UpdatePaymentMetadata
{
    private PaymentController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<PaymentOrchestrator>> _logger;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<PaymentOrchestrator>>();

        var orchestrator = new PaymentOrchestrator(
            _mediator.Object,
            _logger.Object,
            mapper: null);

        _controller = new PaymentController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_When_Update_Succeeds()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaData();

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PaymentMetadata(paymentId, metadata);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(true);
    }


    [Test]
    public async Task PaymentMetadata_When_Request_Is_Null_Returns_BadRequest()
    {
        // Act
        var result = await _controller.PaymentMetadata(Guid.NewGuid(), null);

        // Assert
        result.Should().BeOfType<BadRequestResult>();

        var badRequest = result as BadRequestResult;
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task PaymentMetadata_When_PaymentId_Is_Empty_Returns_BadRequest()
    {
        // Arrange
        var metadata = new PaymentMetaData();

        // Act
        var result = await _controller.PaymentMetadata(Guid.Empty, metadata);

        // Assert
        result.Should().BeOfType<BadRequestResult>();

        var badRequest = result as BadRequestResult;
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task PaymentMetadata_When_Update_Fails_Returns_InternalServerError()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaData();

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.PaymentMetadata(paymentId, metadata);

        // Assert
        result.Should().BeOfType<ObjectResult>();

        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        objectResult.Value.Should().Be("Could not update Payment Metadata");
    }
}
