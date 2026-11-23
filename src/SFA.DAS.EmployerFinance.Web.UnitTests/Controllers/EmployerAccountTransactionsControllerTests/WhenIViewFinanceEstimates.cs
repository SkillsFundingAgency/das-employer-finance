using AutoMapper;
using SFA.DAS.EmployerFinance.Models.FeatureToggle;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

public class WhenIViewFinanceEstimates
{
    private const string HashedAccountId = "ABC123";
    private const long LastUpdated = 638680000000000000;

    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
    private Mock<IFeature> _feature;

    [SetUp]
    public void Arrange()
    {
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();

        _feature = new Mock<IFeature>();
        _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency)).Returns(true);

        _controller = new EmployerAccountTransactionsController(
            _orchestrator.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IMediator>(),
            Mock.Of<IEncodingService>(),
            _feature.Object);
    }

    [Test]
    public async Task ThenNoContentIsReturnedWhenTheEstimatesHaveNotBeenUpdated()
    {
        _orchestrator
            .Setup(x => x.GetEstimatesIfUpdated(HashedAccountId, LastUpdated, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EstimatesViewModel)null);

        var result = await _controller.FinanceEstimates(HashedAccountId, LastUpdated, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task ThenTheEstimatesPartialIsReturnedWhenTheEstimatesHaveBeenUpdated()
    {
        var estimates = new EstimatesViewModel { LastUpdatedUtc = DateTime.UtcNow, MonthEstimates = [] };
        _orchestrator
            .Setup(x => x.GetEstimatesIfUpdated(HashedAccountId, LastUpdated, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estimates);

        var result = await _controller.FinanceEstimates(HashedAccountId, LastUpdated, CancellationToken.None);

        var partialView = result.Should().BeOfType<PartialViewResult>().Subject;
        partialView.ViewName.Should().Be(ViewNames.FinanceDashboardEstimates);
        partialView.Model.Should().BeSameAs(estimates);
    }

    [Test]
    public async Task ThenNotFoundIsReturnedWhenLevyProjectionTransparencyIsTurnedOff()
    {
        _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency)).Returns(false);

        var result = await _controller.FinanceEstimates(HashedAccountId, LastUpdated, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        _orchestrator.Verify(x => x.GetEstimatesIfUpdated(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
