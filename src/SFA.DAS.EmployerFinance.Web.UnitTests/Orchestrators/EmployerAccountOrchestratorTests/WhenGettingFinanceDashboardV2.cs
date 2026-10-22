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
    private const short Months = 6;

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
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Summary = new GetLevySummaryByAccountIdResponse
                {
                    CurrentLevyFunds = 1000M,
                    TotalLevyDeclaredLast12Months = 5000M,
                    TotalCommittedLearnerCosts = 7500M,
                    TotalCommittedTransfersCosts = 2500M
                },
                Projections = new List<MonthlyFundingBreakdown>
                {
                    new()
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary
                        {
                            LevyIn = 1000M,
                            Total = 1000M,
                        },
                        LevyOutSummary = new LevyOutSummary
                        {
                            ExpiredLevy = 200M,
                        }
                    },
                    new()
                    {
                        Month = 9,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary
                        {
                            LevyIn = 2000M,
                            Total = 2000M,
                        },
                        LevyOutSummary = new LevyOutSummary
                        {
                            ExpiredLevy = 400M,
                        }
                    },
                    new()
                    {
                        Month = 10,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary
                        {
                            LevyIn = 3000M,
                            Total = 3000M,
                        },
                        LevyOutSummary = new LevyOutSummary
                        {
                            ExpiredLevy = 600M,
                        }
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
    public async Task ThenTheLevyProjectionsAreRetrievedOnceUsingTheDecodedAccountId()
    {
        await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        _mockOuterApiService.Verify(x => x.GetLevyProjections(AccountId, Months, false, It.IsAny<CancellationToken>()), Times.Once);
        _mockOuterApiService.VerifyNoOtherCalls();
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
    public async Task ThenTheTotalCommittedCostsAreSetFromTheLevySummary()
    {
        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.TotalCommittedLearnerCosts.Should().Be(-7500M);
        result.Data.TotalCommittedTransfersCosts.Should().Be(-2500M);
    }

    [Test]
    public async Task ThenTheTotalCommittedCostsAreNotTheSumOfTheMonthlyProjections()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Summary = new GetLevySummaryByAccountIdResponse
                {
                    TotalCommittedLearnerCosts = 7500M,
                    TotalCommittedTransfersCosts = 2500M
                },
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary(),
                        LevyOutSummary = new LevyOutSummary { CommittedLearnerCost = 100M, CommittedTransferOut = 50M }
                    },
                    new MonthlyFundingBreakdown
                    {
                        Month = 9,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary(),
                        LevyOutSummary = new LevyOutSummary { CommittedLearnerCost = 200M, CommittedTransferOut = 75M }
                    }
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        // the totals cover all remaining committed costs, not just those within the projected months
        result.Data.TotalCommittedLearnerCosts.Should().Be(-7500M).And.NotBe(-300M);
        result.Data.TotalCommittedTransfersCosts.Should().Be(-2500M).And.NotBe(-125M);
    }

    [Test]
    public async Task ThenTheSummaryValuesDefaultToZeroWhenTheProjectionsHaveNoSummary()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse());

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.CurrentLevyFunds.Should().Be(0);
        result.Data.TotalLevyDeclaredLast12Months.Should().Be(0);
        result.Data.TotalCommittedLearnerCosts.Should().Be(0);
        result.Data.TotalCommittedTransfersCosts.Should().Be(0);
        result.Data.Estimates!.MonthEstimates.Should().BeEmpty();
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
            new MonthEstimateViewModel { Period = "August 2026",    LevyIn = 1000M, ExpiredLevy = -200M },
            new MonthEstimateViewModel { Period = "September 2026", LevyIn = 2000M, ExpiredLevy = -400M },
            new MonthEstimateViewModel { Period = "October 2026",   LevyIn = 3000M, ExpiredLevy = -600M }
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
            .Setup(x => x.GetLevyProjections(AccountId, Months))
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
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary
                        {
                            LevyIn = 500M,
                            Total = 500M,
                        },
                        LevyOutSummary = new LevyOutSummary
                        {
                            Total = 175M,
                            ExpiredLevy = 100M,
                            CommittedLearnerCost = 50M,
                            CommittedTransferOut = 25M,
                        },
                        ClosingBalance = 350M,
                    },
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);
        
        result.Data.Estimates!.MonthEstimates.Should().HaveCount(1);
        result.Data.Estimates.MonthEstimates[0].Should().BeEquivalentTo(
            new MonthEstimateViewModel
            {
                Period = "August 2026",
                LevyIn = 500M,
                ExpiredLevy = -100M,
                LevyOut = -175M,
                ClosingLevyBalance = 350M,
                CommittedLearnerCosts = -50M,
                CommittedTransferCosts = -25M
            });
    }

    [Test]
    public async Task Then_MonthEstimates_Costs_Are_Displayed_As_Negative()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary(),
                        LevyOutSummary = new LevyOutSummary
                        {
                            CommittedLearnerCost = 50M,
                            CommittedTransferOut = 25M,
                            CommittedLearnerFinalPaymentCost = 10M,
                            ExpiredLevy = 100M,
                        },
                    },
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        var estimate = result.Data.Estimates!.MonthEstimates.Single();
        estimate.CommittedLearnerCosts.Should().Be(-50M);
        estimate.CommittedTransferCosts.Should().Be(-25M);
        estimate.CommittedLearnerFinalPaymentCost.Should().Be(-10M);
        estimate.ExpiredLevy.Should().Be(-100M);
    }

    [Test]
    public async Task Then_MonthEstimates_LevyIn_LevyOut_And_ClosingBalance_Keep_Their_Sign()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary { Total = 500M },
                        LevyOutSummary = new LevyOutSummary { Total = 175M },
                        ClosingBalance = 350M,
                    },
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        var estimate = result.Data.Estimates!.MonthEstimates.Single();
        estimate.LevyIn.Should().Be(500M);
        estimate.LevyOut.Should().Be(-175M);
        estimate.ClosingLevyBalance.Should().Be(350M);
    }

    [Test]
    public async Task Then_MonthEstimates_Zero_Costs_Are_Displayed_As_Zero()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Projections =
                [
                    new MonthlyFundingBreakdown
                    {
                        Month = 8,
                        Year = 2026,
                        LevyInSummary = new LevyInSummary(),
                        LevyOutSummary = new LevyOutSummary(),
                    },
                ]
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        var estimate = result.Data.Estimates!.MonthEstimates.Single();
        estimate.CommittedLearnerCosts.Should().Be(0);
        estimate.CommittedTransferCosts.Should().Be(0);
        estimate.CommittedLearnerFinalPaymentCost.Should().Be(0);
        estimate.ExpiredLevy.Should().Be(0);
    }

    [Test]
    public async Task ThenTheOtherSummaryTotalsKeepTheirSign()
    {
        _mockOuterApiService
            .Setup(x => x.GetLevyProjections(AccountId, Months))
            .ReturnsAsync(new GetLevyProjectionsByAccountIdResponse
            {
                Summary = new GetLevySummaryByAccountIdResponse
                {
                    CurrentLevyFunds = 1000M,
                    TotalLevyDeclaredLast12Months = 5000M,
                    TotalLevySpentLast12Months = 3000M,
                    TotalLevyExpiredLast12Months = 1500M
                },
                Projections = []
            });

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.CurrentLevyFunds.Should().Be(1000M);
        result.Data.TotalLevyDeclaredLast12Months.Should().Be(5000M);
        result.Data.TotalLevySpentLast12Months.Should().Be(3000M);
        result.Data.TotalLevyExpiredLast12Months.Should().Be(1500M);
    }
}
