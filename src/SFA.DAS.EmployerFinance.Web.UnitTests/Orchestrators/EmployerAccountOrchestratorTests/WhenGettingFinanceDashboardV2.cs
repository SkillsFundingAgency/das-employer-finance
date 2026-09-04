using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Levy;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
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

        _mockEncodingService
            .Setup(x => x.Decode(HashedAccountId, EncodingType.AccountId))
            .Returns(AccountId);

        _mockAccountApiClient
            .Setup(x => x.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy.ToString()
            });

        _mockOuterApiService
            .Setup(x => x.GetLevySummary(HashedAccountId, false))
            .ReturnsAsync(new GetLevySummaryByHashedAccountIdResponse
            {
                CurrentLevyFunds = 1000M,
                TotalLevyDeclaredLast12Months = 5000M
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

        _mockOuterApiService.Verify(x => x.GetLevySummary(HashedAccountId, false), Times.Once);
    }

    [Test]
    public async Task ThenIsLevyEmployerIsTrueWhenAccountTypeIsLevy()
    {
        _mockAccountApiClient
            .Setup(x => x.GetAccount(AccountId))
            .ReturnsAsync(new AccountDetailViewModel
            {
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy.ToString()
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
                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy.ToString()
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
    public async Task ThenShowLevyTransparencyIsSetFromConfiguration()
    {
        _configuration.ShowLevyTransparency = false;

        var result = await _orchestrator.GetFinanceDashboardV2(HashedAccountId);

        result.Data.ShowLevyTransparency.Should().BeFalse();
    }
}