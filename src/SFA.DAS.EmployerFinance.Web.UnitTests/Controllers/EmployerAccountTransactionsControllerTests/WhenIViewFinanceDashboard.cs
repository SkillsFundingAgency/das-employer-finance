using AutoMapper;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

public class WhenIViewFinanceDashboard
{
    private const string ExpectedHashedAccountId = "ABC123";
    private const decimal ExpectedCurrentFunds = 123.45M;

    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
        
    [SetUp]
    public void Arrange()
    {
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
        _orchestrator.Setup(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel
                {
                    HashedAccountId = ExpectedHashedAccountId,
                    CurrentLevyFunds = ExpectedCurrentFunds
                }
            });

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new []
            {
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier,Guid.NewGuid().ToString())
            }
        ));
        _controller = new EmployerAccountTransactionsController(
            _orchestrator.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IMediator>(),
            Mock.Of<IEncodingService>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext {User = user}
        };
    }

    [Test]
    public async Task ThenTheAccountHashedIdIsReturned()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var viewResult = result as ViewResult;
        (viewResult).Should().NotBeNull();

        var model = viewResult.Model as Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>;
        (model).Should().NotBeNull();
        (model.Data).Should().NotBeNull();
        model.Data.HashedAccountId.Should().BeEquivalentTo(ExpectedHashedAccountId);
    }

    [Test]
    public async Task ThenTheViewModelHasTheCorrectLevyBalance()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var viewResult = result as ViewResult;
        (viewResult).Should().NotBeNull();

        var model = viewResult.Model as Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>;
        (model).Should().NotBeNull();
        (model.Data).Should().NotBeNull();
        model.Data.CurrentLevyFunds.Should().Be(ExpectedCurrentFunds);
    }

    [Test]
    public async Task ThenCorrectRedirectResultIsReturnedWhenOrchestratorRequestARedirect()
    {
        //Arrange
        const string redirectUrl = "http://example.com";

        _orchestrator.Setup(o => o.Index(It.IsAny<string>(),It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new Web.Orchestrators.OrchestratorResponse<FinanceDashboardViewModel>
            {
                RedirectUrl = redirectUrl
            });

        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        var redirectResult = result as RedirectResult;
        (redirectResult).Should().NotBeNull();
        redirectResult.Url.Should().Be(redirectUrl);
        redirectResult.Permanent.Should().BeFalse();
    }

    [Test]
    public async Task ThenRedirectResultIsNotReturnedWhenOrchestratorDoesNotRequestARedirect()
    {
        //Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        //Assert
        result.Should().NotBeOfType<RedirectResult>();
    }
}