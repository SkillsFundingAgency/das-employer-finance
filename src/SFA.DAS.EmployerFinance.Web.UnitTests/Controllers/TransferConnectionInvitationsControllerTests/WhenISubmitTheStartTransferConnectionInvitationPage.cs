using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Queries.SendTransferConnectionInvitation;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests;

[TestFixture]
public class WhenISubmitTheStartTransferConnectionInvitationPage
{
    private const string ReceiverAccountPublicHashedId = "XYZ987";

    private TransferConnectionInvitationsController _controller;
    private StartTransferConnectionInvitationViewModel _viewModel;
    private readonly Mock<IMediator> _mediator = new Mock<IMediator>();

    [SetUp]
    public void Arrange()
    {
        _mediator.Setup(m => m.Send(It.IsAny<SendTransferConnectionInvitationQuery>(), CancellationToken.None)).ReturnsAsync(new SendTransferConnectionInvitationResponse());

        _controller = new TransferConnectionInvitationsController(null, _mediator.Object, Mock.Of<IUrlActionHelper>(), Mock.Of<IEncodingService>());

        _viewModel = new StartTransferConnectionInvitationViewModel
        {
            ReceiverAccountPublicHashedId = ReceiverAccountPublicHashedId
        };
    }

    [Test]
    public async Task ThenAGetTransferConnectionInvitationAccountQueryShouldBeSent()
    {
        await _controller.Start(ReceiverAccountPublicHashedId,_viewModel);

        _mediator.Verify(m => m.Send(It.Is<SendTransferConnectionInvitationQuery>(q => q.ReceiverAccountPublicHashedId == _viewModel.ReceiverAccountPublicHashedId), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenIShouldBeRedirectedToTheSendTransferConnectionInvitationPage()
    {
        var result = await _controller.Start(ReceiverAccountPublicHashedId, _viewModel) as RedirectToActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ActionName, Is.EqualTo("Send"));
        Assert.That(result.RouteValues.TryGetValue("ReceiverAccountPublicHashedId", out var receiverPublicHashedAccountId), Is.True);
        Assert.That(receiverPublicHashedAccountId, Is.EqualTo(ReceiverAccountPublicHashedId));
    }
}