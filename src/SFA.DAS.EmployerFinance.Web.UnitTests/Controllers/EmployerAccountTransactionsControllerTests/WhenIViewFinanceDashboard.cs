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
    private const decimal ExpectedTotalLevyDeclaredLast12Months = 678.90M;
    private const decimal ExpectedTotalLevySpentLast12Months = 234.56M;

    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
    private Mock<IFeature> _featureMock;

    [SetUp]
    public void Arrange()
    {
        _featureMock = new Mock<IFeature>();
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
        _orchestrator.Setup(o => o.GetFinanceDashboardV2(ExpectedHashedAccountId))
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardV2ViewModel>
            {
                Data = new FinanceDashboardV2ViewModel
                {
                    HashedAccountId = ExpectedHashedAccountId,
                    CurrentLevyFunds = ExpectedCurrentFunds,
                    TotalLevyDeclaredLast12Months = ExpectedTotalLevyDeclaredLast12Months,
                    TotalLevySpentLast12Months = ExpectedTotalLevySpentLast12Months
                }
            });

        _orchestrator.Setup(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel
                {
                    HashedAccountId = ExpectedHashedAccountId,
                    CurrentLevyFunds = ExpectedCurrentFunds,
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
        var viewModel = new OrchestratorResponse<FinanceDashboardV2ViewModel>
        {
            RedirectUrl = null
        };

        _orchestrator
            .Setup(o => o.GetFinanceDashboardV2(It.IsAny<string>()))
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

    [Test]
    public async Task Index_WhenFeatureDisabled_ShouldCallLegacyOrchestrator()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(false);

        _orchestrator
            .Setup(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel { HashedAccountId = ExpectedHashedAccountId }
            });

        // Act
        await _controller.Index(ExpectedHashedAccountId);

        // Assert
        _orchestrator.Verify(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()), Times.Once);
        _orchestrator.Verify(o => o.GetFinanceDashboardV2(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Index_WhenFeatureEnabled_ShouldNotCallLegacyOrchestrator()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        // Act
        await _controller.Index(ExpectedHashedAccountId);

        // Assert
        _orchestrator.Verify(o => o.Index(It.IsAny<string>(), It.IsAny<ClaimsIdentity>()), Times.Never);
        _orchestrator.Verify(o => o.GetFinanceDashboardV2(ExpectedHashedAccountId), Times.Once);
    }

    [Test]
    public async Task Index_WhenFeatureEnabled_ShouldPassHashedAccountIdToV2Orchestrator()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        // Act
        await _controller.Index(ExpectedHashedAccountId);

        // Assert
        _orchestrator.Verify(o => o.GetFinanceDashboardV2(ExpectedHashedAccountId), Times.Once);
    }

    [Test]
    public async Task Index_WhenFeatureEnabledAndOrchestratorReturnsRedirect_ShouldStillReturnIndexV2View()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        var viewModel = new OrchestratorResponse<FinanceDashboardV2ViewModel>
        {
            RedirectUrl = "http://example.com"  // redirect is ignored in the V2 branch
        };

        _orchestrator
            .Setup(o => o.GetFinanceDashboardV2(ExpectedHashedAccountId))
            .ReturnsAsync(viewModel);

        // Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().Be("IndexV2");
    }

    [Test]
    public async Task Index_WhenFeatureEnabledAndV2ViewModelContainsLevyData_ShouldReturnAllLevyValues()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(true);

        // Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();

        var model = viewResult.Model as OrchestratorResponse<FinanceDashboardV2ViewModel>;
        model.Should().NotBeNull();
        model!.Data.Should().NotBeNull();
        model.Data.CurrentLevyFunds.Should().Be(ExpectedCurrentFunds);
        model.Data.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
        model.Data.TotalLevySpentLast12Months.Should().Be(ExpectedTotalLevySpentLast12Months);
    }

    [Test]
    public async Task Index_WhenFeatureDisabled_ShouldReturnDefaultView()
    {
        // Arrange
        _featureMock
            .Setup(f => f.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency))
            .Returns(false);

        _orchestrator
            .Setup(o => o.Index(ExpectedHashedAccountId, It.IsAny<ClaimsIdentity>()))
            .ReturnsAsync(new OrchestratorResponse<FinanceDashboardViewModel>
            {
                Data = new FinanceDashboardViewModel { HashedAccountId = ExpectedHashedAccountId }
            });

        // Act
        var result = await _controller.Index(ExpectedHashedAccountId);

        // Assert
        var viewResult = result as ViewResult;
        viewResult.Should().NotBeNull();
        viewResult!.ViewName.Should().BeNullOrEmpty(); // default view, not named "IndexV2"
    }
}