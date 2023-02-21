using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Transfers;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class WhenGettingTransfersCounts
    {
        private TransfersOrchestrator _orchestrator;
        private Mock<IEmployerAccountAuthorisationHandler> _authorisationService;
        private Mock<IEncodingService> _encodingService;
        private Mock<ITransfersService> _transfersService;
        private Mock<IAccountApiClient> _accountApiClient;

        private const string HashedAccountId = "123ABC";
        private const long AccountId = 1234;
        
        [SetUp]
        public void Setup()
        {
            _authorisationService = new Mock<IEmployerAccountAuthorisationHandler>();
            _encodingService = new Mock<IEncodingService>();
            _transfersService = new Mock<ITransfersService>();
            _accountApiClient = new Mock<IAccountApiClient>();

            _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);            
            
            _orchestrator = new TransfersOrchestrator(_authorisationService.Object, _encodingService.Object, _transfersService.Object, _accountApiClient.Object);
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

            _authorisationService.Setup(o => o.CheckUserAccountAccess(It.IsAny<ClaimsPrincipal>(),Authentication.EmployerUserRole.Transactor)).Returns(isAuthorised);

            var actual = await _orchestrator.GetIndexViewModel(HashedAccountId);

            Assert.AreEqual(expected, actual.Data.RenderCreateTransfersPledgeButton);
        }

        private void SetupTheAccountApiClient(bool isLevy = false)
        {
           var modelToReturn = new AccountDetailViewModel
           {
               ApprenticeshipEmployerType = isLevy ? "Levy" : "NonLevy"
           };
           
           _accountApiClient.Setup(o => o.GetAccount(HashedAccountId)).ReturnsAsync(modelToReturn);
        }
    }
}
