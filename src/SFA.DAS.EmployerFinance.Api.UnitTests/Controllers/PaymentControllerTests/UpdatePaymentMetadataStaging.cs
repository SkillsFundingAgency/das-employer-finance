using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Models.Payments;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class UpdatePaymentMetadata
{
    private PaymentMetaDataController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        var orchestrator = new PaymentMetaDataOrchestrator(_mediator.Object);
        _controller = new PaymentMetaDataController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_When_Update_Succeeds()
    {
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging();

        var response = new PaymentMetaDataResponse
        {
            Upserted = true,
            MetadataId = 1,
            IsSuccess = true
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        result.Should().BeOfType<OkObjectResult>();

        var ok = result as OkObjectResult;
        ok!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var model = ok.Value!
                      .GetType()
                      .GetProperties()
                      .ToDictionary(p => p.Name, p => p.GetValue(ok.Value!));

        ((bool)model["upserted"]).Should().BeTrue();
    }

    [Test]
    public async Task Then_Sends_App_Unit_Metadata_Fields_To_The_Orchestrator()
    {
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging
        {
            LearningType = "ApprenticeshipUnit",
            CourseCode = "ST0001",
            CohortId = 123456
        };

        _mediator
            .Setup(m => m.Send(
                It.Is<UpdatePaymentMetadataStagingCommand>(command =>
                    command.PaymentId == paymentId
                    && command.PaymentMetadataStaging.LearningType == "ApprenticeshipUnit"
                    && command.PaymentMetadataStaging.CourseCode == "ST0001"
                    && command.PaymentMetadataStaging.CohortId == 123456),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentMetaDataResponse
            {
                Upserted = true,
                MetadataId = 1,
                IsSuccess = true
            });

        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        result.Should().BeOfType<OkObjectResult>();
        _mediator.Verify(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task When_Request_Is_Null_Returns_BadRequest()
    {
        var result = await _controller.PaymentMetaDataStaging(Guid.NewGuid(), null);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = result as BadRequestObjectResult;
        badRequest!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequest.Value.Should().Be("Request body is required.");
    }

    [Test]
    public async Task When_Response_Has_ValidationErrors_Returns_BadRequest()
    {
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging();

        var response = new PaymentMetaDataResponse
        {
            HasValidationErrors = true,
            ValidationErrors = new List<string> { "Error" }
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task When_Response_Is_NotFound_Returns_NotFound()
    {
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging();

        var response = new PaymentMetaDataResponse
        {
            NotFound = true
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task When_Response_Is_Not_Success_Returns_500()
    {
        var paymentId = Guid.NewGuid();
        var metadata = new PaymentMetaDataStaging();

        var response = new PaymentMetaDataResponse
        {
            IsSuccess = false
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<UpdatePaymentMetadataStagingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.PaymentMetaDataStaging(paymentId, metadata);

        result.Should().BeOfType<ObjectResult>();

        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        obj.Value.Should().Be("Could not update Payment Metadata Staging");
    }
}
