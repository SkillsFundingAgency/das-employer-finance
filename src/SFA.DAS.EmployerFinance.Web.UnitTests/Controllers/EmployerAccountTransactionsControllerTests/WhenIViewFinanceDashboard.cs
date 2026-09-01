using AutoMapper;
using SFA.DAS.EmployerFinance.Infrastructure;
using SFA.DAS.EmployerFinance.Models.FeatureToggle;
using SFA.DAS.EmployerFinance.Services.Contracts;
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
    private Mock<IFeature> _featureMock;

    [SetUp]
    public void Arrange()
    {
        _featureMock = new Mock<IFeature>();
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
        _orchestrator.Setup(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel
                {
                    HashedAccountId = ExpectedHashedAccountId,
                    CurrentLevyFunds = ExpectedCurrentFunds
                }
            });

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(EmployerClaims.IdamsUserIdClaimTypeIdentifier,Guid.NewGuid().ToString())
            ]
        ));
        _controller = new EmployerAccountTransactionsController(
            _orchestrator.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IMediator>(),
            Mock.Of<IEncodingService>(),
            _featureMock.Object);
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

        var model = viewResult.Model as OrchestratorResponse<FinanceDashboardViewModel>;
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

        var model = viewResult.Model as OrchestratorResponse<FinanceDashboardViewModel>;
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
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardViewModel>
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

    [Test]
    public async Task Index_WhenFeatureEnabledAndNoRedirect_ShouldReturnIndexV2View()
    {
        // Arrange
        var viewModel = new OrchestratorResponse<FinanceDashboardViewModel>
        {
            RedirectUrl = null
        };

        _orchestrator
            .Setup(o => o.Index(It.IsAny<string>(), It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(viewModel);

        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        // Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        // Assert
        var viewResult = result as ViewResult;
        Assert.That(viewResult, Is.Not.Null);
        Assert.That(viewResult.ViewName, Is.EqualTo("IndexV2"));
        Assert.That(viewResult.Model, Is.EqualTo(viewModel));
    }
}