using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Transfers;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

[TestFixture]
public class WhenGettingTransfersCounts
{
    private TransfersOrchestrator _orchestrator;
    private Mock<IEmployerAccountAuthorisationHandler> _authorisationService;
    private Mock<IEncodingService> _encodingService;
    private Mock<ITransfersService> _transfersService;
    private Mock<IAccountApiClient> _accountApiClient;
    private EmployerFinanceConfiguration _configuration;

    private const string HashedAccountId = "123ABC";
    private const long AccountId = 1234;
    private const decimal TransferAllowancePercentage = 0.50m;

    [SetUp]
    public void Setup()
    {
        _authorisationService = new Mock<IEmployerAccountAuthorisationHandler>();
        _encodingService = new Mock<IEncodingService>();
        _transfersService = new Mock<ITransfersService>();
        _accountApiClient = new Mock<IAccountApiClient>();
        _configuration = new EmployerFinanceConfiguration { TransferAllowancePercentage = TransferAllowancePercentage };

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _orchestrator = new TransfersOrchestrator(
            _authorisationService.Object,
            _encodingService.Object,
            _transfersService.Object,
            _accountApiClient.Object,
            _configuration,
            Mock.Of<ILogger<TransfersOrchestrator>>());
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Only_Levy_Payer_Can_View_Pledges_Section(bool isLevyPayer, bool expectIsLevyEmployer)
    {
        _transfersService.Setup(o => o.GetCounts(AccountId)).ReturnsAsync(new GetCountsResponse());

        SetupTheAccountApiClient(isLevyPayer);
            
        var actual = await _orchestrator.GetIndexViewModel(HashedAccountId);

        Assert.AreEqual(expectIsLevyEmployer, actual.Data.IsLevyEmployer);
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task ThenChecksTheUserIsAuthorisedToCreateTransfers(bool isAuthorised, bool expected)
    {
        _transfersService.Setup(o => o.GetCounts(AccountId)).ReturnsAsync(new GetCountsResponse());

        SetupTheAccountApiClient(true);

        _authorisationService.Setup(o => o.CheckUserAccountAccess(EmployerUserRole.Transactor)).ReturnsAsync(isAuthorised);

        var actual = await _orchestrator.GetIndexViewModel(HashedAccountId);

        Assert.AreEqual(expected, actual.Data.RenderCreateTransfersPledgeButton);
    }

    [TestCase(10000, 9000, 1000)]
    public async Task ThenChecksEstimatedRemainingAllowanceCalculation(decimal startingAllowance, decimal currentEstimatedSpend, decimal expected)
    {
        GetCountsResponse countResponse = new GetCountsResponse { CurrentYearEstimatedCommittedSpend = currentEstimatedSpend };

        _transfersService.Setup(o => o.GetCounts(AccountId)).ReturnsAsync(countResponse);

        SetupTheAccountApiClient(true, startingAllowance);

        _authorisationService.Setup(o => o.CheckUserAccountAccess(EmployerUserRole.Transactor)).ReturnsAsync(true);

        var actual = await _orchestrator.GetIndexViewModel(HashedAccountId);

        Assert.AreEqual(expected, actual.Data.EstimatedRemainingAllowance);
    }

    [TestCase(10000, 5000, true)]
    [TestCase(10000, 9000, false)]
    public async Task ThenChecksIfUserHasMinimumTransferFunds(decimal startingAllowance, decimal currentEstimatedSpend, bool expected)
    {
        GetCountsResponse countResponse = new GetCountsResponse {CurrentYearEstimatedCommittedSpend = currentEstimatedSpend };

        _transfersService.Setup(o => o.GetCounts(AccountId)).ReturnsAsync(countResponse);

        SetupTheAccountApiClient(true, startingAllowance);

        _authorisationService.Setup(o => o.CheckUserAccountAccess(EmployerUserRole.Transactor)).ReturnsAsync(true);

        var actual = await _orchestrator.GetIndexViewModel(HashedAccountId);

        Assert.AreEqual(expected, actual.Data.HasMinimumTransferFunds);
    }

    private void SetupTheAccountApiClient(bool isLevy = false, decimal? startingAllowance = null)
    {
        var modelToReturn = new AccountDetailViewModel
        {
            ApprenticeshipEmployerType = isLevy ? "Levy" : "NonLevy",
            StartingTransferAllowance = startingAllowance?? 0
        };
           
        _accountApiClient.Setup(o => o.GetAccount(HashedAccountId)).ReturnsAsync(modelToReturn);
    }
}