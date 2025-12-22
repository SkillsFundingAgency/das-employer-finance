using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferControllerTests;

[TestFixture]
public class WhenIGetTransfersByPeriodEnd
{
    private TransferController _transferController;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<TransferOrchestrator>> _logger;
    private Mock<IMapper> _mapper;

    private const long AccountId = 123;
    private const string PeriodEnd = "2024-01-01";

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<TransferOrchestrator>>();
        _mapper = new Mock<IMapper>();

        var orchestrator = new TransferOrchestrator(
            _mediator.Object,
            _logger.Object,
            _mapper.Object);

        _transferController = new TransferController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Ok_When_Transfers_Exist()
    {
        // Arrange
        var transfers = new List<AccountTransfer>
        {
            new() { SenderAccountId = AccountId }
        };

        var response = new GetTransfersByPeriodEndResponse
        {
            AccountTransfers = transfers
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetTransfersByPeriodEndRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _transferController.GetTransfersByPeriodEnd(AccountId, PeriodEnd);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        var model = ((OkObjectResult)result).Value as GetTransfersByPeriodEndResponse;
        model.Should().NotBeNull();
        model!.AccountTransfers.Count.Should().Be(1);
    }

    [Test]
    public async Task Then_Returns_NotFound_When_No_Transfers_Exist()
    {
        // Arrange
        var response = new GetTransfersByPeriodEndResponse
        {
            AccountTransfers = new List<AccountTransfer>()
        };

        _mediator
            .Setup(x => x.Send(It.IsAny<GetTransfersByPeriodEndRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _transferController.GetTransfersByPeriodEnd(AccountId, PeriodEnd);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task Then_Returns_BadRequest_When_PeriodEnd_Is_Missing()
    {
        // Act
        var result = await _transferController.GetTransfersByPeriodEnd(AccountId, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Then_Returns_BadRequest_When_PeriodEnd_Is_Invalid()
    {
        // Act
        var result = await _transferController.GetTransfersByPeriodEnd(AccountId, "not-a-date");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
