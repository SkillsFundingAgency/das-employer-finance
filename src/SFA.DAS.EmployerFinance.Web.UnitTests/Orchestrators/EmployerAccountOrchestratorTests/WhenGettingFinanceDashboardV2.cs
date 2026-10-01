using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;
using static SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy.GetLevyProjectionsByAccountIdResponse;
using ApprenticeshipEmployerType = SFA.DAS.Common.Domain.Types.ApprenticeshipEmployerType;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators.EmployerAccountOrchestratorTests;

[TestFixture]
internal class WhenGettingFinanceDashboardV2
{
    private Mock<IAccountApiClient> _mockAccountApiClient;
    private Mock<IMediator> _mockMediator;
    private Mock<ICurrentDateTime> _mockCurrentTime;
    private Mock<ILogger<EmployerAccountTransactionsOrchestrator>> _mockLogger;
    private Mock<IEncodingService> _mockEncodingService;
    private Mock<IAuthenticationOrchestrator> _mockAuthenticationOrchestrator;
    private Mock<IGovAuthEmployerAccountService> _mockAccountService;
    private Mock<IOuterApiService> _mockOuterApiService;
    private EmployerAccountTransactionsOrchestrator _orchestrator;
    private EmployerFinanceWebConfiguration _configuration;

    private const string HashedAccountId = "ABC123";
    private const long AccountId = 123L;

    private DateTime _lastRefreshDateTime;

    [SetUp]
    public void Arrange()
    {
        _mockAccountApiClient = new Mock<IAccountApiClient>();
        _mockMediator = new Mock<IMediator>();
        _mockCurrentTime = new Mock<ICurrentDateTime>();
        _mockLogger = new Mock<ILogger<EmployerAccountTransactionsOrchestrator>>();
        _mockEncodingService = new Mock<IEncodingService>();
        _mockAuthenticationOrchestrator = new Mock<IAuthenticationOrchestrator>();
        _mockAccountService = new Mock<IGovAuthEmployerAccountService>();
        _mockOuterApiService = new Mock<IOuterApiService>();
        _configuration = new EmployerFinanceWebConfiguration { ShowLevyTransparency = true };
        _lastRefreshDateTime = DateTime.UtcNow;

        _mockEncodingService
            .Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId))
            .Returns(AccountId);

        _mockAccountApiClient
            .Setup(x => x.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = nameof(ApprenticeshipEmployerType.Levy)
            });

        _mockOuterApiService
            .Setup(x => x.GetLevySummary(AccountId, false))
            .ReturnsAsync(new GetLevySummaryByAccountIdResponse
            {
                CurrentLevyFunds = 1000M,
                TotalLevyDeclaredLast12Months = 5000M
            });

        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, 6))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections = new List<MonthlyBreakdown>
                {
                    new()
                    {
                        LevyIn = 1000M,
                        ExpiredLevy = 200M,
                        CalendarMonthName = "August",
                        CalendarPeriodMonth = 8,
                        CalendarPeriodYear = 2026
                    },
                    new()
                    {
                        LevyIn = 2000M,
                        ExpiredLevy = 400M,
                        CalendarMonthName = "September",
                        CalendarPeriodMonth = 9,
                        CalendarPeriodYear = 2026
                    },
                    new()
                    {
                        LevyIn = 3000M,
                        ExpiredLevy = 600M,
                        CalendarMonthName = "October",
                        CalendarPeriodMonth = 10,
                        CalendarPeriodYear = 2026
                    }
                },
                LastRefreshDateTime = _lastRefreshDateTime,
            });

        _mockCurrentTime
            .Setup(x => x.Now)
            .Returns(new DateTime(2026, 08, 01));

        _orchestrator = new EmployerAccountTransactionsOrchestrator(
            _mockAccountApiClient.Object,
            _mockMediator.Object,
            _mockCurrentTime.Object,
            _mockLogger.Object,
            _mockEncodingService.Object,
            _mockAuthenticationOrchestrator.Object,
            _mockAccountService.Object,
            _mockOuterApiService.Object,
            _configuration);
    }

    [Test]
    public async Task ThenTheAccountIdIsDecodedFromTheHashedAccountId()
    {
        await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        _mockEncodingService.Verify(x => x.Decode(HashedAccountId, EncodingType.AccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheAccountDetailsAreRetrievedUsingTheDecodedAccountId()
    {
        await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        _mockAccountApiClient.Verify(x => x.GetAccount(AccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheLevySummaryIsRetrievedUsingTheHashedAccountId()
    {
        await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        _mockOuterApiService.Verify(x => x.GetLevySummary(AccountId, false), Times.Once);
    }

    [Test]
    public async Task ThenIsLevyEmployerIsTrueWhenAccountTypeIsLevy()
    {
        _mockAccountApiClient
            .Setup(x => x.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = nameof(ApprenticeshipEmployerType.Levy)
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.IsLevyEmployer.Should().BeTrue();
    }

    [Test]
    public async Task ThenIsLevyEmployerIsFalseWhenAccountTypeIsNonLevy()
    {
        _mockAccountApiClient
            .Setup(x => x.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = nameof(ApprenticeshipEmployerType.NonLevy)
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.IsLevyEmployer.Should().BeFalse();
    }

    [Test]
    public async Task ThenTheHashedAccountIdIsSetOnTheViewModel()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.HashedAccountId.Should().Be(HashedAccountId);
    }

    [Test]
    public async Task ThenTheCurrentLevyFundsAreSetFromTheLevySummary()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.CurrentLevyFunds.Should().Be(1000M);
    }

    [Test]
    public async Task ThenTheTotalLevyDeclaredLast12MonthsIsSetFromTheLevySummary()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.TotalLevyDeclaredLast12Months.Should().Be(5000M);
    }

    [Test]
    public async Task Then_MonthEstimates_Count_Matches_Projections()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates.Should().HaveCount(3);
    }

    [Test]
    public async Task Then_MonthEstimates_Are_Mapped_From_Projections()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates.Should().BeEquivalentTo(
        [
            new MonthEstimateViewModel { Period = "August 2026",    LevyIn = 1000M, ExpiredLevy = 200M},
            new MonthEstimateViewModel { Period = "September 2026", LevyIn = 2000M, ExpiredLevy = 400M },
            new MonthEstimateViewModel { Period = "October 2026",   LevyIn = 3000M, ExpiredLevy = 600M }
        ], options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_MonthEstimates_Preserves_Order_From_Projections()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates[0].Period.Should().Be("August 2026");
        result.Data.Estimates.MonthEstimates[1].Period.Should().Be("September 2026");
        result.Data.Estimates.MonthEstimates[2].Period.Should().Be("October 2026");
    }

    [Test]
    public async Task Then_MonthEstimates_Maps_LevyIn_Correctly()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates[0].LevyIn.Should().Be(1000M);
        result.Data.Estimates.MonthEstimates[1].LevyIn.Should().Be(2000M);
        result.Data.Estimates.MonthEstimates[2].LevyIn.Should().Be(3000M);
    }

    [Test]
    public async Task Then_MonthEstimates_Maps_MonthName_Correctly()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates[0].Period.Should().Be("August 2026");
        result.Data.Estimates.MonthEstimates[1].Period.Should().Be("September 2026");
        result.Data.Estimates.MonthEstimates[2].Period.Should().Be("October 2026");
    }

    [Test]
    public async Task Then_DateTimeNow_Maps_LastRefreshDate_Correctly()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.LastUpdatedUtc.Should().Be(_lastRefreshDateTime);
    }

    [Test]
    public async Task Then_MonthEstimates_Is_Empty_When_No_Projections()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, 6))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections = []
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates.Should().BeEmpty();
    }

    [Test]
    public async Task Then_MonthEstimates_Maps_Single_Projection_Correctly()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, 6))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections =
                [
                    new MonthlyBreakdown { LevyIn = 500M, ExpiredLevy = 100M, CalendarMonthName = "August", CalendarPeriodMonth = 8, CalendarPeriodYear = 2026 },
                    new MonthlyBreakdown { LevyIn = 1000M, ExpiredLevy = 200M, CalendarMonthName = "September", CalendarPeriodMonth = 9, CalendarPeriodYear = 2026 },
                    new MonthlyBreakdown { LevyIn = 1500M, ExpiredLevy = 300M, CalendarMonthName = "October", CalendarPeriodMonth = 10, CalendarPeriodYear = 2026 }
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.Estimates!.MonthEstimates.Should().HaveCount(3);
        result.Data.Estimates.MonthEstimates[0].Should().BeEquivalentTo(
            new MonthEstimateViewModel { Period = "August 2026", LevyIn = 500M, ExpiredLevy = 100M });
        result.Data.Estimates.MonthEstimates[1].Should().BeEquivalentTo(
            new MonthEstimateViewModel { Period = "September 2026", LevyIn = 1000M, ExpiredLevy = 200M });
        result.Data.Estimates.MonthEstimates[2].Should().BeEquivalentTo(
            new MonthEstimateViewModel { Period = "October 2026", LevyIn = 1500M, ExpiredLevy = 300M });
    }
}