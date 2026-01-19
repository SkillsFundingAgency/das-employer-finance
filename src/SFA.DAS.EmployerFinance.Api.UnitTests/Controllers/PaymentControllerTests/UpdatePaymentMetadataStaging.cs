using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class UpdatePaymentMetadata
{
    private PaymentController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        var orchestrator = new PaymentOrchestrator(
            _mediator.Object);

        _controller = new PaymentController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_When_Update_Succeeds()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging();

        var response = new PaymentMetaDataResponse
        {
            Upserted = true,
            MetadataId = 1
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var model = okResult.Value as PaymentMetaDataResponse;
        model.Should().NotBeNull();
        model!.Upserted.Should().BeTrue();
        model.MetadataId.Should().Be(1);
    }


    [Test]
    public async Task PaymentMetadata_When_Request_Is_Null_Returns_BadRequest()
    {
        // Act
        var result = await _controller.PaymentMetaDataStaging(Guid.NewGuid(), null);

        // Assert
        result.Should().BeOfType<BadRequestResult>();

        var badRequest = result as BadRequestResult;
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task PaymentMetadata_When_PaymentId_Is_Empty_Returns_BadRequest()
    {
        // Arrange
        var metadata = new PaymentMetaDataStaging();

        // Act
        var result = await _controller.PaymentMetaDataStaging(Guid.Empty, metadata);

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
        var metadata = new PaymentMetaDataStaging();

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

        var objectResult = result as BadRequestObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        objectResult.Value.Should().Be("Could not update Payment Metadata Staging");
    }
}
