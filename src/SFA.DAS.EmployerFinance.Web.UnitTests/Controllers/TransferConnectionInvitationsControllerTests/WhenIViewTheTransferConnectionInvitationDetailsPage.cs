using AutoMapper;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Mappings;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests;

[TestFixture]
public class WhenIViewTheTransferConnectionInvitationDetailsPage
{
    private TransferConnectionInvitationsController _controller;
    private IConfigurationProvider _configurationProvider;
    private IMapper _mapper;
    private Mock<IMediator> _mediator;
    private readonly GetTransferConnectionInvitationQuery _query = new GetTransferConnectionInvitationQuery();
    private GetTransferConnectionInvitationResponse _response = new GetTransferConnectionInvitationResponse();

    private const long AccountId = 12345;
    private const string HashedAccountId = "ABC123";
    private const long TransferConnectionId = 54321;
    private const string HashedTransferConnectionId = "XYZ345";
        
    [SetUp]
    public void Arrange()
    {
        var fixture = new Fixture();
        _response = fixture.Create<GetTransferConnectionInvitationResponse>();
        _configurationProvider = new MapperConfiguration(c => c.AddProfile<TransferMappings>());
        _mapper = _configurationProvider.CreateMapper();
        _mediator = new Mock<IMediator>();

        var encodingService = new Mock<IEncodingService>();
        encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
        encodingService.Setup(x => x.Decode(HashedTransferConnectionId, EncodingType.TransferRequestId)).Returns(TransferConnectionId);

        _mediator.Setup(m =>
            m.Send(
                It.Is<GetTransferConnectionInvitationQuery>(c =>
                    c.AccountId.Equals(AccountId) && c.TransferConnectionInvitationId.Equals(TransferConnectionId)),
                CancellationToken.None)).ReturnsAsync(_response);

        _controller = new TransferConnectionInvitationsController(_mapper, _mediator.Object,null, encodingService.Object);
    }

    [Test]
    public async Task ThenAGetTransferConnectionQueryShouldBeSent()
    {
        await _controller.Details(HashedAccountId, HashedTransferConnectionId);

        _mediator.Verify(m => m.Send(It.Is<GetTransferConnectionInvitationQuery>(c=>c.AccountId.Equals(AccountId) && c.TransferConnectionInvitationId.Equals(TransferConnectionId) ), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenIShouldBeShownTheTransferConnectionDetailsPage()
    {
        var result = await _controller.Details(HashedAccountId, HashedTransferConnectionId) as ViewResult;
        var model = result?.Model as TransferConnectionInvitationViewModel;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ViewName, Is.Null);
        Assert.That(model, Is.Not.Null);
    }
}