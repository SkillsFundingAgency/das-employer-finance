using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.TransferConnectionsControllerTests;

[TestFixture]
public class WhenIGetTransferConnections
{
    private TransferConnectionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<IEncodingService> _encodingService;
    private GetTransferConnectionsResponse _response;
    private IEnumerable<TransferConnection> _transferConnections;
    private const string HashedAccountId = "GF3XWP";
    private const string PublicHashedAccountId = "DJ7JL";
    private const int AccountId = 123;
    private readonly TransferConnectionInvitationStatus PendingStatus = TransferConnectionInvitationStatus.Pending;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _transferConnections = new List<TransferConnection>
        {
            new TransferConnection { FundingEmployerAccountId = AccountId, FundingEmployerAccountName = "ACCOUNT NAME", FundingEmployerHashedAccountId = HashedAccountId, FundingEmployerPublicHashedAccountId = PublicHashedAccountId }
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
    public async Task ThenGetTransferConnectionsQueryShouldBeSentWithDecodedHashedAccountId()
    {
        await _controller.GetTransferConnections(HashedAccountId);

        _mediator.Verify(
            m => m.Send(It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(AccountId)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ThenGetTransferConnectionsQueryShouldBeSentWithAccountId()
    {
        await _controller.GetTransferConnections(AccountId);

        _mediator.Verify(
            m => m.Send(It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(AccountId)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ThenGetTransferConnectionsQueryShouldBeSentWithAccountId_AndStatusPopulated()
    {
        await _controller.GetTransferConnections(AccountId, PendingStatus);

        _mediator.Verify(
            m => m.Send(It.Is<GetTransferConnectionsQuery>(q => q.AccountId.Equals(AccountId)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ThenShouldReturnTransferConnectionsForDecodedHashedAccountId()
    {
        var result = await _controller.GetTransferConnections(HashedAccountId) as OkObjectResult;

        result.Should().NotBeNull();

        Assert.That(result.Value, Is.SameAs(_transferConnections));
    }

    [Test]
    public async Task ThenShouldReturnTransferConnectionsForAccountId()
    {
        var result = await _controller.GetTransferConnections(AccountId) as OkObjectResult;

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.SameAs(_transferConnections));
    }

    [Test]
    public async Task ThenShouldReturnTransferConnectionsForAccountId_AndStatusPopulated()
    {
        var result = await _controller.GetTransferConnections(AccountId, PendingStatus) as OkObjectResult;

        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.SameAs(_transferConnections));
    }
}