using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Helpers;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.AccountTransactionsControllerTests;

public class WhenIGetTransactionsForAnAccount
{
    private AccountTransactionsController _controller;
    private Mock<IMediator> _mediator;
    private Mock<ILogger<AccountTransactionsOrchestrator>> _logger;
    private Mock<ILinkGeneratorWrapper> _linkGenerator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<AccountTransactionsOrchestrator>>();
        _linkGenerator = new Mock<ILinkGeneratorWrapper>();
           
        var orchestrator = new AccountTransactionsOrchestrator(_mediator.Object, _logger.Object, _linkGenerator.Object);
        _controller = new AccountTransactionsController(orchestrator, _linkGenerator.Object);
    }

    [Test]
    public async Task ThenTheTransactionsAreReturned()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var year = 2017;
        var month = 3;
        var transactionsResponse = new GetEmployerAccountTransactionsResponse
        {                
            Data = TransactionLineObjectMother.Create(),
            AccountHasPreviousTransactions = false
        };
        _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);
           
        //Act
        var response = await _controller.GetTransactions(hashedAccountId, year, month);

        //Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOf<OkObjectResult>(response);
        var model = ((OkObjectResult)response).Value as Transactions;

        model?.Should().NotBeNull();
        transactionsResponse.Data?.TransactionLines.Should().BeEquivalentTo(model, options => options.Excluding(x => x.ResourceUri));
    }

    [Test]
    public async Task AndThereAreNoPreviousTransactionThenTheUrlIsNotSet()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var year = 2017;
        var month = 3;           
        var transactionsResponse = new GetEmployerAccountTransactionsResponse
        {
            Data = TransactionLineObjectMother.Create(),
            AccountHasPreviousTransactions = false
        };
        _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

        //Act
        var response = await _controller.GetTransactions(hashedAccountId, year, month);
            
        //Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOf<OkObjectResult>(response);
        var model = ((OkObjectResult)response).Value as Transactions;

        model?.Should().NotBeNull();
        model?.PreviousMonthUri.Should().BeNullOrEmpty();
    }

    [Test]
    public async Task AndThereArePreviousTransactionsThenTheLinkIsCorrect()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var year = 2017;
        var month = 1;            
        var transactionsResponse = new GetEmployerAccountTransactionsResponse
        {
            Data = TransactionLineObjectMother.Create(),
            AccountHasPreviousTransactions = true,
            Year = year,
            Month = month
        };
        _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

        //Act
        var expectedUri = "someuri";

        _linkGenerator.Setup(
                x => x.GetPathByName(
                    "GetTransactions",
                    It.Is<object>(o => o.IsEquivalentTo(new
                    {
                        hashedAccountId,
                        year = year - 1,
                        month = 12
                    }))))
            .Returns(expectedUri);

        //Assert
        var response = await _controller.GetTransactions(hashedAccountId, year, month);
        var model = ((OkObjectResult)response).Value as Transactions;

        model?.PreviousMonthUri.Should().Be(expectedUri);
    }

    //[Test]
    //public async Task AndNoMonthIsProvidedThenTheCurrentMonthIsUsed()
    //{
    //    //Arrange
    //    var hashedAccountId = "ABC123";
    //    var year = 2017;           
    //    var transactionsResponse = new GetEmployerAccountTransactionsResponse
    //    {
    //        Data = TransactionLineObjectMother.Create(),
    //        AccountHasPreviousTransactions = false
    //    };
    //    _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == DateTime.Now.Month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

    //    //Act
    //    var response = await _controller.GetTransactions(hashedAccountId, year);

    //    //Assert
    //    Assert.IsNotNull(response);
    //    Assert.IsInstanceOf<OkObjectResult>(response);
    //    var model = ((OkObjectResult)response).Value as Transactions;

    //    model?.Should().NotBeNull();
    //    model?.Should().BeEquivalentTo(transactionsResponse.Data.TransactionLines, opts => opts.ExcludingMissingMembers());
    //    model?.PreviousMonthUri.Should().BeNullOrEmpty();

    //    _urlHelper.Setup(x => x.RouteUrl(
    //        It.Is<UrlRouteContext>(c =>
    //            c.RouteName == "GetTransactions")));
    //}


    [Test]
    public async Task AndThereAreLevyTransactionsThenTheLinkIsCorrect()
    {
        //Arrange
        var hashedAccountId = "ABC123";
        var year = 2017;
        var month = 1;
        var levyTransaction = TransactionLineObjectMother.Create();
        var transactionsResponse = new GetEmployerAccountTransactionsResponse
        {
            Data = TransactionLineObjectMother.Create(),
            AccountHasPreviousTransactions = false,
            Year = year,
            Month = month
        };
        _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == year && q.Month == month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

        var expectedUri = "someuri";

        _linkGenerator.Setup(
                x => x.GetPathByName(
                    "GetLevyForPeriod",
                    It.Is<object>(o => o.IsEquivalentTo(new
                    {
                        hashedAccountId = hashedAccountId,
                        payrollYear = levyTransaction.TransactionLines[0].PayrollYear,
                        payrollMonth = levyTransaction.TransactionLines[0].PayrollMonth
                    }))))
            .Returns(expectedUri);

        //Act
        var response = await _controller.GetTransactions(hashedAccountId, year, month);
        var model = ((OkObjectResult)response).Value as Transactions;

        //Assert            
        model? [0].ResourceUri.Should().Be(expectedUri);
    }


    //[Test]
    //public async Task AndNoYearIsProvidedThenTheCurrentYearIsUsed()
    //{
    //    //Arrange
    //    var hashedAccountId = "ABC123";           
    //    var transactionsResponse = new GetEmployerAccountTransactionsResponse
    //    {
    //        Data = TransactionLineObjectMother.Create(),
    //        AccountHasPreviousTransactions = false
    //    };
    //    _mediator.Setup(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(q => q.HashedAccountId == hashedAccountId && q.Year == DateTime.Now.Year && q.Month == DateTime.Now.Month), It.IsAny<CancellationToken>())).ReturnsAsync(transactionsResponse);

    //    //Act
    //    var response = await _controller.GetTransactions(hashedAccountId);

    //    //Assert            
    //    Assert.IsNotNull(response);
    //    Assert.IsInstanceOf<OkObjectResult>(response);
    //    var model = ((OkObjectResult)response).Value as Transactions;

    //    model?.Should().NotBeNull();
    //    model?.Should().BeEquivalentTo(transactionsResponse.Data.TransactionLines, opts => opts.ExcludingMissingMembers());
    //}
}