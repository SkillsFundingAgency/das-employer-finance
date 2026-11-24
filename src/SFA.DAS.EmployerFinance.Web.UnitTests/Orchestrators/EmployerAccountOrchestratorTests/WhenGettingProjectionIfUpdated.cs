using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators.EmployerAccountOrchestratorTests;

[TestFixture]
internal class WhenGettingProjectionIfUpdated
{
    private const string HashedAccountId = "ABC123";
    private const long AccountId = 123L;
    private const short Months = 6;

    private static readonly DateTime LoadedImportDate = new(2026, 11, 23, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime NewImportDate = LoadedImportDate.AddSeconds(3);

    private Mock<IOuterApiService> _mockOuterApiService;
    private Mock<IAccountApiClient> _mockAccountApiClient;
    private EmployerAccountTransactionsOrchestrator _orchestrator;

    [SetUp]
    public void Arrange()
    {
        _mockOuterApiService = new Mock<IOuterApiService>();
        _mockAccountApiClient = new Mock<IAccountApiClient>();

        var mockEncodingService = new Mock<IEncodingService>();
        mockEncodingService.Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                LastRefreshDateTime = NewImportDate,
                Summary = new GetLevySummaryByAccountIdResponse
                {
                    CurrentLevyFunds = 1000M,
                    TotalCommittedLearnerCosts = 7500M,
                    TotalCommittedTransfersCosts = 2500M
                },
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 11,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary { Total = 1000.10M },
                        LevyOutSummary = new LevyOutSummary { ExpiredLevy = 200M },
                        ClosingBalance = 800M
                    }
                ]
            });

        _orchestrator = new EmployerAccountTransactionsOrchestrator(
            _mockAccountApiClient.Object,
            Mock.Of<IMediator>(),
            Mock.Of<ICurrentDateTime>(),
            Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(),
            mockEncodingService.Object,
            Mock.Of<IAuthenticationOrchestrator>(),
            Mock.Of<IGovAuthEmployerAccountService>(),
            _mockOuterApiService.Object,
            new EmployerFinanceWebConfiguration());
    }

    private void SetupStatus(DateTime lastSuccessfulImportDate) =>
        _mockOuterApiService
            .Setup(x => x.GetLevyProjectionsStatus(AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyProjectionsStatusByAccountIdResponse { LastSuccessfulImportDate = lastSuccessfulImportDate });

    [Test]
    public async Task ThenNullIsReturnedAndTheProjectionsAreNotFetchedWhenTheImportDateHasNotChanged()
    {
        SetupStatus(LoadedImportDate);

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.Should().BeNull();
        _mockOuterApiService.Verify(x => x.GetLevyProjections(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenTheImportDateIsOlderThanTheOneLoaded()
    {
        SetupStatus(LoadedImportDate.AddMinutes(-1));

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.Should().BeNull();
    }

    [Test]
    public async Task ThenTheLatestEstimatesAreReturnedWhenTheImportDateHasChanged()
    {
        SetupStatus(NewImportDate);

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.Should().NotBeNull();
        result.HashedAccountId.Should().Be(HashedAccountId);
        result.Estimates!.LastUpdatedUtc.Should().Be(NewImportDate);
        result.Estimates.MonthEstimates.Should().BeEquivalentTo(
        [
            new MonthEstimateViewModel { Period = "November 2026", LevyIn = 1001M, ExpiredLevy = -200M, ClosingLevyBalance = 800M }
        ]);
        _mockOuterApiService.Verify(x => x.GetLevyProjections(AccountId, Months, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenTheLatestCommitmentsAreReturnedAsNegativeWhenTheImportDateHasChanged()
    {
        SetupStatus(NewImportDate);

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.TotalCommittedLearnerCosts.Should().Be(-7500M);
        result.TotalCommittedTransfersCosts.Should().Be(-2500M);
    }

    [Test]
    public async Task ThenTheLevySummaryIsNotRefreshedAsItDoesNotComeFromTheProjection()
    {
        SetupStatus(NewImportDate);

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.CurrentLevyFunds.Should().Be(0);
    }

    [Test]
    public async Task ThenTheCommitmentsAreZeroWhenTheProjectionsHaveNoSummary()
    {
        SetupStatus(NewImportDate);
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse { LastRefreshDateTime = NewImportDate });

        var result = await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        result.TotalCommittedLearnerCosts.Should().Be(0);
        result.TotalCommittedTransfersCosts.Should().Be(0);
        result.Estimates!.MonthEstimates.Should().BeEmpty();
    }

    [Test]
    public async Task ThenTheProjectionsAreFetchedWithoutTriggeringAnotherRefresh()
    {
        SetupStatus(NewImportDate);

        await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        _mockOuterApiService.Verify(x => x.GetLevyProjections(AccountId, Months, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ThenTheAccountDetailsAreNotRetrieved()
    {
        SetupStatus(NewImportDate);

        await _orchestrator.GetProjectionIfUpdated(HashedAccountId, LoadedImportDate.Ticks);

        _mockAccountApiClient.VerifyNoOtherCalls();
    }
}
