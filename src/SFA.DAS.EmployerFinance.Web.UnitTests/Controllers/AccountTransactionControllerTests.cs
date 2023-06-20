using AutoMapper;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Web.Controllers;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.EmployerFinance.Web.ViewModels;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Controllers;

public class AccountTransactionControllerTests
{
    private EmployerAccountTransactionsController _controller;
    private Mock<IEmployerAccountTransactionsOrchestrator> _orchestrator;


    [SetUp]
    public void Arrange()
    {
        _orchestrator = new Mock<IEmployerAccountTransactionsOrchestrator>();

        _orchestrator.Setup(x => x.GetAccountTransactions(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Web.Orchestrators.OrchestratorResponse<TransactionViewResultViewModel>
            {
                Data = new TransactionViewResultViewModel(DateTime.Now)
                {
                    Account = new EAS.Account.Api.Types.AccountDetailViewModel(),
                    Model = new TransactionViewModel
                    {
                        Data = new AggregationData()
                    },
                    AccountHasPreviousTransactions = true
                }
            });

        _controller = new Web.Controllers.EmployerAccountTransactionsController(
            _orchestrator.Object, Mock.Of<IMapper>(), Mock.Of<IMediator>(), Mock.Of<IEncodingService>());
    }

    [Test]
    public async Task ThenTransactionsAreRetrievedForTheAccount()
    {
        //Act
        var result = await _controller.TransactionsView("TEST", 2017, 1);

        //Assert
        _orchestrator.Verify(
            x => x.GetAccountTransactions(It.Is<string>(s => s == "TEST"), It.IsAny<int>(), It.IsAny<int>()),
            Times.Once);
        Assert.IsNotNull(result as ViewResult);
    }

    [Test]
    public async Task ThenPrevioussTransactionsStatusIsShown()
    {
        //Act
        var result = await _controller.TransactionsView("TEST", 2017, 1);

        var viewResult = result as ViewResult;
        var viewModel = viewResult?.Model as Web.Orchestrators.OrchestratorResponse<TransactionViewResultViewModel>;

        //Assert
        Assert.IsNotNull(viewModel);
        Assert.IsTrue(viewModel.Data.AccountHasPreviousTransactions);
    }
}