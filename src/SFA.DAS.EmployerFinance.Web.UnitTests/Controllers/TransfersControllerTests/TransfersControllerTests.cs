using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.Infrastructure.OuterApiResponses.Transfers;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.TransfersControllerTests;

public class TransfersControllerTests
{
    private TransfersController _controller;
    private TransfersOrchestrator _orchestrator;
    private Mock<IEncodingService> _encodingService;
    private Mock<IEmployerAccountAuthorisationHandler> _authorisationService;
    private Mock<ITransfersService> _transfersService;
    private Mock<IAccountApiClient> _accountApiClient;
    private GetFinancialBreakdownResponse _financialBreakdownResponse;
    private AccountDetailViewModel _accountDetailViewModel;
    private EmployerFinanceConfiguration _configuration;

    private const string HashedAccountId = "123ABC";
    private const long AccountId = 1234;
    private const decimal TransferAllowancePercentage = 0.50m;

    [SetUp]
    public void Arrange()
    {
        var fixture = new Fixture();

        _authorisationService = new Mock<IEmployerAccountAuthorisationHandler>();
        _encodingService = new Mock<IEncodingService>();
        _transfersService = new Mock<ITransfersService>();
        _accountApiClient = new Mock<IAccountApiClient>();
        _financialBreakdownResponse = fixture.Create<GetFinancialBreakdownResponse>();
        _configuration = new EmployerFinanceConfiguration { TransferAllowancePercentage = TransferAllowancePercentage };

        _transfersService.Setup(m => m.GetFinancialBreakdown(AccountId)).ReturnsAsync(_financialBreakdownResponse);

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);
        _accountDetailViewModel = new AccountDetailViewModel();
        _accountApiClient.Setup(m => m.GetAccount(HashedAccountId)).ReturnsAsync(_accountDetailViewModel);
        
        _orchestrator = new TransfersOrchestrator(
            _authorisationService.Object, 
            _encodingService.Object,
            _transfersService.Object,
            _accountApiClient.Object,
            _configuration,
            Mock.Of<ILogger<TransfersOrchestrator>>());

        _controller = new TransfersController(_orchestrator);
    }

    [Test]
    public async Task FinancialBreakdownReturnsAViewModelWithData()
    {   
        var result = await _controller.FinancialBreakdown(HashedAccountId);
        var view = result as ViewResult;
        var viewModel = view?.Model as OrchestratorResponse<FinancialBreakdownViewModel>;
           
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Data);
        Assert.IsNotNull(viewModel.Data.AcceptedPledgeApplications);
        Assert.IsNotNull(viewModel.Data.ApprovedPledgeApplications);
        Assert.IsNotNull(viewModel.Data.Commitments);
        Assert.IsNotNull(viewModel.Data.TransferConnections);
        Assert.IsNotNull(viewModel.Data.CurrentYearEstimatedSpend);
        Assert.IsNotNull(viewModel.Data.NextYearEstimatedSpend);
        Assert.IsNotNull(viewModel.Data.YearAfterNextYearEstimatedSpend);
    }        

    [Test]
    public async Task FinancialBreakdownPageShowsEstimatedRemainingAllowance()
    {
        var viewModel = await GetViewModel();

        var estimatedRemainingAllowance = viewModel.Data.StartingTransferAllowance - viewModel.Data.CurrentYearEstimatedSpend;
        Assert.AreEqual(estimatedRemainingAllowance, viewModel.Data.EstimatedRemainingAllowance);
    }

    [Test]
    public async Task FinancialBreakdownPageShowsCorrectTotalAvailablePledgedFunds()
    {
        var viewModel = await GetViewModel();

        var totalAvailablePledgedFunds = viewModel.Data.TotalAvailableTransferAllowance - viewModel.Data.TotalPledgedAndTransferConnections;
        Assert.AreEqual(totalAvailablePledgedFunds, viewModel.Data.TotalAvailablePledgedFunds);
    }

    [Test]
    public async Task FinancialBreakdownPageShowsCorrectTotalPledgedAndTransferConnections()
    {
        var viewModel = await GetViewModel();

        var totalPledgedAndTransferConnections = viewModel.Data.AmountPledged + viewModel.Data.TransferConnections;
        Assert.AreEqual(totalPledgedAndTransferConnections, viewModel.Data.TotalPledgedAndTransferConnections);
    }

    [Test]
    public async Task FinancialBreakdownPageShowsCorrectAvailablePledgedFunds()
    {
        var viewModel = await GetViewModel();

        var availablePledgedFunds = viewModel.Data.AmountPledged - (viewModel.Data.ApprovedPledgeApplications + viewModel.Data.AcceptedPledgeApplications);
        Assert.AreEqual(availablePledgedFunds, viewModel.Data.AvailablePledgedFunds);
    }

    private async Task<OrchestratorResponse<FinancialBreakdownViewModel>> GetViewModel()
    {
        var result = await _controller.FinancialBreakdown(HashedAccountId);

        var view = result as ViewResult;
        var viewModel = view?.Model as OrchestratorResponse<FinancialBreakdownViewModel>;

        Assert.IsNotNull(viewModel);

        return viewModel;
    }
}