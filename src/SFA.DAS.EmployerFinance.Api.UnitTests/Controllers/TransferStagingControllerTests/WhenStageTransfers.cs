using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferStagingControllerTests;

public class StageTransfersTests
{
    private Mock<TransferStagingOrchestrator> _orchestrator;
    private TransferStagingController _controller;

    [SetUp]
    public void Arrange()
    {
        _orchestrator = new Mock<TransferStagingOrchestrator>();
        _controller = new TransferStagingController(_orchestrator.Object);
    }

    [Test]
    public async Task StageTransfers_WhenRequestIsNull_ReturnsBadRequest()
    {
        var result = await _controller.StageTransfers(null);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequest.Value.Should().Be("Transfers payload is required.");
    }

    [Test]
    public async Task StageTransfers_WhenTransfersCollectionIsEmpty_ReturnsBadRequest()
    {
        var request = new BulkTransferStagingRequest
        {
            Transfers = new List<TransferStaging>()
        };

        var result = await _controller.StageTransfers(request);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequest.Value.Should().Be("Transfers payload is required.");
    }

    [Test]
    public async Task StageTransfers_WhenValidationErrorsExist_ReturnsBadRequest()
    {
        var request = new BulkTransferStagingRequest
        {
            Transfers = new List<TransferStaging> { new() }
        };

        var response = new BulkTransferStagingResponse
        {
            HasValidationErrors = true,
            ValidationErrors = new List<string> { "Invalid transfer" }
        };

        _orchestrator
            .Setup(x => x.StageTransfers(It.IsAny<BulkTransferStagingRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.StageTransfers(request);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        badRequest.Value.Should().BeEquivalentTo(response.ValidationErrors);
    }

    [Test]
    public async Task StageTransfers_WhenConflictsExist_ReturnsConflict()
    {
        var request = new BulkTransferStagingRequest
        {
            Transfers = new List<TransferStaging> { new() }
        };

        var response = new BulkTransferStagingResponse
        {
            HasConflicts = true,
            ConflictingTransferIds = new List<long> { 123, 456 }
        };

        _orchestrator
            .Setup(x => x.StageTransfers(It.IsAny<BulkTransferStagingRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.StageTransfers(request);

        result.Should().BeOfType<ConflictObjectResult>();

        var conflict = (ConflictObjectResult)result;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        conflict.Value.Should().BeEquivalentTo(new
        {
            transferIds = response.ConflictingTransferIds
        });
    }

    [Test]
    public async Task StageTransfers_WhenSuccessful_ReturnsCreated()
    {
        var request = new BulkTransferStagingRequest
        {
            Transfers = new List<TransferStaging> { new(), new() }
        };

        var response = new BulkTransferStagingResponse
        {
            InsertedCount = 2,
            TransferIds = new List<long> { 1, 2 }
        };

        _orchestrator
            .Setup(x => x.StageTransfers(It.IsAny<BulkTransferStagingRequest>()))
            .ReturnsAsync(response);

        var result = await _controller.StageTransfers(request);

        result.Should().BeOfType<ObjectResult>();

        var created = (ObjectResult)result;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().BeEquivalentTo(new
        {
            insertedCount = 2,
            transferIds = new List<long> { 1, 2 }
        });
    }
}
