using AutoMapper;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.StagingControllerTests;

public class BulkPaymentsIngestTests
{
    private PaymentsController _stagingController;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<StagingOrchestrator>> _logger;
    private Mock<IMapper> _mapper;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<StagingOrchestrator>>();
        _mapper = new Mock<IMapper>();

        var orchestrator = new StagingOrchestrator(_mediator.Object, _logger.Object);
        _stagingController = new PaymentsController(orchestrator);
    }

    [Test]
    public async Task BulkPaymentsIngest_ReturnsOk_WhenOrchestratorSucceeds()
    {
        // Arrange
        var payments = new List<PaymentStagingModel>
        {
            new PaymentStagingModel
            {
                PaymentId = Guid.NewGuid(),
                AccountId = 123,
                Ukprn = 12345678,
                Uln = 1234567890,
                ApprenticeshipId = 456,
                CollectionPeriodId = "R01",
                DeliveryPeriodMonth = 1,
                DeliveryPeriodYear = 2025,
                CollectionPeriodMonth = 1,
                CollectionPeriodYear = 2025,
                Amount = 100
            }
        };

        var response = new BulkPaymentsIngestResponse();
        response.IsSuccess = true;
        response.PaymentIds = payments.Select(p => p.PaymentId).ToList();

        _mediator
            .Setup(m => m.Send(It.IsAny<BulkPaymentsIngestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _stagingController.BulkPaymentsIngest(payments);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedResult>();
        (result as CreatedResult)!.StatusCode.Should().Be(StatusCodes.Status201Created);
    }


    [Test]
    public async Task BulkPaymentsIngest_ReturnsBadRequest_WhenPaymentsIsNull()
    {
        // Act
        var result = await _stagingController.BulkPaymentsIngest(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        (result as BadRequestObjectResult)!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task BulkPaymentsIngest_ReturnsInternalServerError_WhenOrchestratorFails()
    {
        // Arrange
        var payments = new List<PaymentStagingModel>
    {
        new PaymentStagingModel
        {
            PaymentId = Guid.NewGuid(),
            Amount = 100,
            AccountId = 123,
            Ukprn = 45678901,
            Uln = 9876543210,
            ApprenticeshipId = 12345,
            CollectionPeriodId = "R01",
            DeliveryPeriodMonth = 1,
            DeliveryPeriodYear = 2026,
            CollectionPeriodMonth = 1,
            CollectionPeriodYear = 2026
        }
    };

        var response = new BulkPaymentsIngestResponse();
        response.IsSuccess = false;

        _mediator
            .Setup(m => m.Send(It.IsAny<BulkPaymentsIngestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _stagingController.BulkPaymentsIngest(payments);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }


}
