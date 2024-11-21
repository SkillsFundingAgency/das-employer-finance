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
    private EmployerFinanceConfiguration _configuration;

    private const string HashedAccountId = "123ABC";
    private const long AccountId = 1234;
    private const decimal TransferAllowancePercentage = 0.50m;

    [SetUp]
    public void Setup()
    {
        var fixture = new Fixture();

        _encodingService = new Mock<IEncodingService>();
        _transfersService = new Mock<ITransfersService>();
        _accountApiClient = new Mock<IAccountApiClient>();
        _financialBreakdownResponse = fixture.Create<GetFinancialBreakdownResponse>();
        _configuration = new EmployerFinanceConfiguration { TransferAllowancePercentage = TransferAllowancePercentage };

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _accountApiClient.Setup(m => m.GetAccount(HashedAccountId)).ReturnsAsync(new AccountDetailViewModel
        {
            AccountId = AccountId
        });


        _orchestrator = new TransfersOrchestrator( 
            Mock.Of<IEmployerAccountAuthorisationHandler>(),
            _encodingService.Object,
            _transfersService.Object,
            _accountApiClient.Object,
            _configuration,
            Mock.Of<ILogger<TransfersOrchestrator>>());
    }
    [Test]
    public async Task CheckFinancialBreakdownViewModel()
    {
        _transfersService.Setup(o => o.GetFinancialBreakdown(AccountId)).ReturnsAsync(_financialBreakdownResponse);

        var actual = await _orchestrator.GetFinancialBreakdownViewModel(HashedAccountId);

        actual.Data.AcceptedPledgeApplications.Should().Be(_financialBreakdownResponse.AcceptedPledgeApplications + _financialBreakdownResponse.PledgeOriginatedCommitments);
        actual.Data.ApprovedPledgeApplications.Should().Be(_financialBreakdownResponse.ApprovedPledgeApplications);
        actual.Data.Commitments.Should().Be(_financialBreakdownResponse.Commitments);
        actual.Data.TransferConnections.Should().Be(_financialBreakdownResponse.TransferConnections);
        actual.Data.PledgeOriginatedCommitments.Should().Be(_financialBreakdownResponse.PledgeOriginatedCommitments);
    }
}