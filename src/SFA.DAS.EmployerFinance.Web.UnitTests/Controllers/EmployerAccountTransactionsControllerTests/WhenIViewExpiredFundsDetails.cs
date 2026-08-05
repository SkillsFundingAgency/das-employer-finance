using AutoMapper;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Helpers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers.EmployerAccountTransactionsControllerTests;

public class WhenIViewExpiredFundsDetails
{
    private const string HashedAccountId = "ABC123";
    private readonly DateTime _fromDate = DateTime.Now.AddDays(-30);
    private readonly DateTime _toDate = DateTime.Now.AddDays(-1);

    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;
    private OrchestratorResponse<ExpiredFundsTransactionDetailsViewModel> _orchestratorResponse;

    [SetUp]
    public void Arrange()
    {
        _orchestratorResponse = new OrchestratorResponse<ExpiredFundsTransactionDetailsViewModel>
        {
            Data = new ExpiredFundsTransactionDetailsViewModel
            {
                HashedAccountId = HashedAccountId,
                Total = 150m,
                TwelveMonthExpiryAmount = 50m,
                TwentyFourthMonthExpiryAmount = 100m,
                TransactionDate = DateTime.Now.AddDays(-5)
            }
        };

        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();
        _orchestrator
            .Setup(o => o.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate))
            .ReturnsAsync(_orchestratorResponse);

        _controller = new EmployerAccountTransactionsController(
            _orchestrator.Object,
            Mock.Of<IMapper>(),
            Mock.Of<IMediator>(),
            Mock.Of<IEncodingService>());
    }

    [Test]
    public async Task ThenAViewResultIsReturned()
    {
        //Act
        var result = await _controller.ExpiredFundsDetails(HashedAccountId, _fromDate, _toDate);

        //Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Test]
    public async Task ThenTheOrchestratorIsCalledWithCorrectParameters()
    {
        //Act
        await _controller.ExpiredFundsDetails(HashedAccountId, _fromDate, _toDate);

        //Assert
        _orchestrator.Verify(o => o.FindAccountExpiredFunds(HashedAccountId, _fromDate, _toDate), Times.Once);
    }

    [Test]
    public async Task ThenTheCorrectViewNameIsUsed()
    {
        //Act
        var result = await _controller.ExpiredFundsDetails(HashedAccountId, _fromDate, _toDate) as ViewResult;

        //Assert
        result.ViewName.Should().Be(ControllerConstants.ExpiredFundsDetailViewName);
    }

    [Test]
    public async Task ThenTheViewModelIsTheOrchestratorResponse()
    {
        //Act
        var result = await _controller.ExpiredFundsDetails(HashedAccountId, _fromDate, _toDate) as ViewResult;

        //Assert
        result.Model.Should().Be(_orchestratorResponse);
    }

    [Test]
    public async Task ThenTheViewModelContainsTheCorrectData()
    {
        //Act
        var result = await _controller.ExpiredFundsDetails(HashedAccountId, _fromDate, _toDate) as ViewResult;
        var model = result.Model as OrchestratorResponse<ExpiredFundsTransactionDetailsViewModel>;

        //Assert
        model.Data.HashedAccountId.Should().Be(HashedAccountId);
        model.Data.Total.Should().Be(150m);
        model.Data.TwelveMonthExpiryAmount.Should().Be(50m);
        model.Data.TwentyFourthMonthExpiryAmount.Should().Be(100m);
    }
}
