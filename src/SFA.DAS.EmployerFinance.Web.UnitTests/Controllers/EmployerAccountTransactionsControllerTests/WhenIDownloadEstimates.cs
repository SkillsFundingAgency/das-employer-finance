using AutoMapper;
using SFA.DAS.EmployerFinance.Formatters.TransactionDowloads;
using SFA.DAS.EmployerFinance.Models.FeatureToggle;
using SFA.DAS.EmployerFinance.Queries.GetEstimatesDownload;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Infrastructure;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

public class WhenIDownloadEstimates
{
    private const string HashedAccountId = "ABC123";
    private const long AccountId = 123;

    private EmployerAccountTransactionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<IFeature> _feature;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _mediator
            .Setup(x => x.Send(It.IsAny<GetEstimatesDownloadQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEstimatesDownloadResponse { FileData = [1, 2, 3], FileExtension = "csv", MimeType = "text/csv" });

        _feature = new Mock<IFeature>();
        _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency)).Returns(true);

        var encodingService = new Mock<IEncodingService>();
        encodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _controller = new EmployerAccountTransactionsController(
            Mock.Of<IEmployerAccountTransactionsOrchestrator>(),
            Mock.Of<IMapper>(),
            _mediator.Object,
            encodingService.Object,
            _feature.Object);
    }

    [TestCase(DownloadFormatType.CSV)]
    [TestCase(DownloadFormatType.Excel)]
    public async Task ThenTheEstimatesAreDownloadedInTheChosenFormat(DownloadFormatType format)
    {
        var result = await _controller.EstimatesDownload(HashedAccountId, format, CancellationToken.None) as FileContentResult;

        result.Should().NotBeNull();
        result.FileContents.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        result.ContentType.Should().Be("text/csv");
        result.FileDownloadName.Should().StartWith("esfaEstimates_").And.EndWith(".csv");
        _mediator.Verify(x => x.Send(
            It.Is<GetEstimatesDownloadQuery>(q => q.AccountId == AccountId && q.DownloadFormat == format),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenTheUserIsReturnedToTheDashboardWhenNoFormatIsChosen()
    {
        var result = await _controller.EstimatesDownload(HashedAccountId, null, CancellationToken.None) as RedirectToRouteResult;

        result.Should().NotBeNull();
        result.RouteName.Should().Be(RouteNames.FinanceIndex);
        _mediator.Verify(x => x.Send(It.IsAny<GetEstimatesDownloadQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ThenNotFoundIsReturnedWhenTheProjectionFeatureIsOff()
    {
        _feature.Setup(x => x.IsFeatureEnabled(FeatureNames.LevyProjectionTransparency)).Returns(false);

        var result = await _controller.EstimatesDownload(HashedAccountId, DownloadFormatType.CSV, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }
}
