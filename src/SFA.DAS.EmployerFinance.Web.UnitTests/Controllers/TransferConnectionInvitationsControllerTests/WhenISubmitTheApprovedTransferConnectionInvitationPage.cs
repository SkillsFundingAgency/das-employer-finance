using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests;

[TestFixture]
public class WhenISubmitTheApprovedTransferConnectionInvitationPage
{
    private const string HashedAccountId = "ABC123";

    private TransferConnectionInvitationsController _controller;
    private readonly ApprovedTransferConnectionInvitationViewModel _viewModel = new ApprovedTransferConnectionInvitationViewModel();
    private readonly Mock<IMediator> _mediator = new Mock<IMediator>();

    [SetUp]
    public void Arrange()
    {
        var urlHelper = new Mock<IUrlActionHelper>();
        urlHelper.Setup(x => x.EmployerAccountsAction("teams")).Returns($"/accounts/{HashedAccountId}/teams");
        urlHelper.Setup(x => x.EmployerCommitmentsV2Action("")).Returns($"/{HashedAccountId}");
            
        _controller = new TransferConnectionInvitationsController(null, _mediator.Object, urlHelper.Object, Mock.Of<IEncodingService>());
    }

    [Test]
    public void ThenIShouldBeRedirectedToTheApprenticesPageIfIChoseOption1()
    {
        _viewModel.Choice = "GoToApprenticesPage";

        var result = _controller.Approved(HashedAccountId, "",_viewModel) as RedirectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Url, Is.EqualTo($"/{HashedAccountId}"));
    }

    [Test]
    public void ThenIShouldBeRedirectedToTheHomepageIfIChoseOption2()
    {
        _viewModel.Choice = "GoToHomepage";

        var result = _controller.Approved(HashedAccountId, "", _viewModel) as RedirectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Url, Is.EqualTo($"/accounts/{HashedAccountId}/teams"));
    }
}