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
public class WhenGettingTransfersFinancialBreakdown
{
    private TransfersOrchestrator _orchestrator;
    private Mock<IEncodingService> _encodingService;
    private Mock<ITransfersService> _transfersService;
    private Mock<IAccountApiClient> _accountApiClient;
    private GetFinancialBreakdownResponse _financialBreakdownResponse;
    public EmployerFinanceConfiguration _employerFinanceConfiguration;

    private const string HashedAccountId = "123ABC";
    private const long AccountId = 1234;

    [SetUp]
    public void Setup()
    {
        var fixture = new Fixture();

        _encodingService = new Mock<IEncodingService>();
        _transfersService = new Mock<ITransfersService>();
        _accountApiClient = new Mock<IAccountApiClient>();
        _financialBreakdownResponse = fixture.Create<GetFinancialBreakdownResponse>();

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _accountApiClient.Setup(m => m.GetAccount(HashedAccountId)).ReturnsAsync(new AccountDetailViewModel
        {
            AccountId = AccountId
        });

        _employerFinanceConfiguration = new EmployerFinanceConfiguration()
        {
            MinimumTransferFunds = 2000
        };

        _orchestrator = new TransfersOrchestrator(_employerFinanceConfiguration, Mock.Of<IEmployerAccountAuthorisationHandler>(), _encodingService.Object, _transfersService.Object, _accountApiClient.Object);
    }
    [Test]
    public async Task CheckFinancialBreakdownViewModel()
    {
        _transfersService.Setup(o => o.GetFinancialBreakdown(AccountId)).ReturnsAsync(_financialBreakdownResponse);

        var actual = await _orchestrator.GetFinancialBreakdownViewModel(HashedAccountId);

        Assert.AreEqual(_financialBreakdownResponse.AcceptedPledgeApplications + _financialBreakdownResponse.PledgeOriginatedCommitments, actual.Data.AcceptedPledgeApplications);
        Assert.AreEqual(_financialBreakdownResponse.ApprovedPledgeApplications, actual.Data.ApprovedPledgeApplications);
        Assert.AreEqual(_financialBreakdownResponse.Commitments, actual.Data.Commitments);
        Assert.AreEqual(_financialBreakdownResponse.TransferConnections, actual.Data.TransferConnections);
        Assert.AreEqual(_financialBreakdownResponse.PledgeOriginatedCommitments, actual.Data.PledgeOriginatedCommitments);
    }
}