using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Models.Transfers;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIGetTransfersByPeriodEnd
{
    private TransferConnectionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<IEncodingService> _encodingService;
    private GetTransferConnectionsResponse _response;
    private IEnumerable<TransferConnection> _transferConnections;
    private const string HashedAccountId = "GF3XWP";
    private const int AccountId = 123;
    private const TransferConnectionInvitationStatus PendingStatus = TransferConnectionInvitationStatus.Pending;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _transferConnections = new List<TransferConnection>
        {
            new() { FundingEmployerAccountId = AccountId, FundingEmployerAccountName = "ACCOUNT NAME" }
        };

        _response = new GetTransferConnectionsResponse { TransferConnections = _transferConnections };

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _mediator.Setup(
                m => m.Send(
                    It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(AccountId)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_response);

        _mediator.Setup(
                m => m.Send(
                    It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(AccountId) && q.Status == PendingStatus), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_response);

        _controller = new TransferConnectionsController(_mediator.Object, _encodingService.Object);
    }

    [Test]
    public async Task ThenGetTransfersByPeriodEndQueryShouldBeSentWithCorrectValues()
    {
        var periodEnd = "Test";
        await _controller.GetTransfersByPeriodEnd(AccountId, periodEnd);

        _mediator.Verify(
            m => m.Send(
                It.Is<GetTransfersByPeriodEndRequest>(q =>
                    q.AccountId == AccountId &&
                    q.PeriodEnd == periodEnd),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

  
    [Test]
    public async Task ThenShouldReturnEmptyListWhenNoTransfersFound()
    {
        // Arrange (override)
        var emptyResponse = new GetTransfersByPeriodEndResponse
        {
            AccountTransfers = new List<AccountTransfer>()
        };

        _mediator
            .Setup(m => m.Send(
                It.Is<GetTransfersByPeriodEndRequest>(q =>
                    q.AccountId == AccountId &&
                    q.PeriodEnd == "EMPTY"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResponse);

        // Act
        var result = await _controller.GetTransfersByPeriodEnd(AccountId, "EMPTY") as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().BeSameAs(emptyResponse);
        emptyResponse.AccountTransfers.Should().BeEmpty();
    }
}
