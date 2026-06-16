using AutoMapper;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using SFA.DAS.EmployerFinance.Models.Transfers;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferStagingControllerTests;

[TestFixture]
public class StageTransfersTests
{
    private TransferStagingController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<TransferStagingOrchestrator>> _logger;
    private Mock<IMapper> _mapper;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<TransferStagingOrchestrator>>();
        _mapper = new Mock<IMapper>();

        var orchestrator = new TransferStagingOrchestrator(
            _mediator.Object,
            _logger.Object,
            _mapper.Object);

        _controller = new TransferStagingController(orchestrator);
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
        var request = new StageTransfersRequest
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
        var request = new StageTransfersRequest
        {
            Transfers = new List<TransferStaging> { new() }
        };

        var response = new StageTransfersResponse
        {
            HasValidationErrors = true,
            ValidationErrors = new List<string> { "Invalid transfer" }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<StageTransfersCommand>(), It.IsAny<CancellationToken>()))
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
        var request = new StageTransfersRequest
        {
            Transfers = new List<TransferStaging> { new() }
        };

        var response = new StageTransfersResponse
        {
            HasConflicts = true,
            ConflictingTransferIds = new List<long> { 123, 456 }
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<StageTransfersCommand>(), It.IsAny<CancellationToken>()))
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
        var request = new StageTransfersRequest
        {
            Transfers = new List<TransferStaging> { new(), new() }
        };

        var response = new StageTransfersResponse
        {
            InsertedTransferIds = new List<long> { 1, 2 },
            IsSuccess = true
        };

        _mediator
            .Setup(x => x.Send(
                It.Is<StageTransfersCommand>(c => c.Transfers.Count == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.StageTransfers(request);

        result.Should().BeOfType<ObjectResult>();

        var created = (ObjectResult)result;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.Value.Should().BeEquivalentTo(new
        {
            insertedCount = request.Transfers.Count,
            transferIds = response.InsertedTransferIds
        });
    }


}
