using DocumentFormat.OpenXml.Bibliography;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EmployerFinance.Interfaces;
using SFA.DAS.EmployerFinance.Models.Account;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountTransactions;
using SFA.DAS.EmployerFinance.Services;
using SFA.DAS.EmployerFinance.Web.Orchestrators;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Orchestrators;

public class WhenIGetAccountTransactions
{
    private const string HashedAccountId = "123ABC";
    private const string ExternalUser = "Test user";
    private const long AccountId = 1234;

    private Mock<IAccountApiClient> _accountApiClient;
    private Mock<IMediator> _mediator;
    private EmployerAccountTransactionsOrchestrator _orchestrator;
    private GetEmployerAccountResponse _response;
    private Mock<ICurrentDateTime> _currentTime;
    private Mock<IEncodingService> _encodingService;

    [SetUp]
    public void Arrange()
    {
        _accountApiClient = new Mock<IAccountApiClient>();
        _mediator = new Mock<IMediator>();
        _currentTime = new Mock<ICurrentDateTime>();
        _encodingService = new Mock<IEncodingService>();

        _response = new GetEmployerAccountResponse
        {
            Account = new Account
            {
                Name = "Test Account"
            }
        };

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        _mediator.Setup(x => x.Send(It.IsAny<GetEmployerAccountHashedQuery>(), CancellationToken.None))
            .ReturnsAsync(_response);

        _encodingService.Setup(h => h.Decode(HashedAccountId, EncodingType.AccountId)).Returns(AccountId);

        SetupGetTransactionsResponse(2017, 5);

        _orchestrator = new EmployerAccountTransactionsOrchestrator(_accountApiClient.Object, _mediator.Object, _currentTime.Object, Mock.Of<ILogger<EmployerAccountTransactionsOrchestrator>>(), Mock.Of<IEncodingService>(),Mock.Of<IAuthenticationOrchestrator>(),Mock.Of<IGovAuthEmployerAccountService>());
    }

    [Test]
    [TestCase(2, 2017)]
    [TestCase(6, 2017)]
    [TestCase(8, 2019)]
    [TestCase(12, 2020)]
    public async Task ThenARequestShouldBeMadeForTransactions(int month, int year)
    {
        //Act
        var fromDate = new DateTime(year, month, 1);
        var toDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

        await _orchestrator.GetAccountTransactions(HashedAccountId, year, month);

        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(
            q => q.FromDate == fromDate && q.ToDate == toDate), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenARequestShouldBeMadeForTransactionsForCurrentMonthIfNoYearOrMonthHasBeenGiven()
    {
        //Act
        await _orchestrator.GetAccountTransactions(HashedAccountId, default(int), default(int));

        //Assert
        _mediator.Verify(x => x.Send(It.Is<GetEmployerAccountTransactionsQuery>(
            q => q.FromDate.Year == DateTime.Now.Year
            && q.FromDate.Month == DateTime.Now.Month
            && q.FromDate.Day == 1
            && q.ToDate.Year == DateTime.Now.Year
            && q.ToDate.Month == DateTime.Now.Month
            && q.ToDate.Day == DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)
            ), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task ThenResultShouldHaveYearAndMonthOfRequest()
    {
        //Arrange
        const int year = 2016;
        const int month = 2;
        SetupGetTransactionsResponse(year, month);

        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, year, month);

        //Assert
        result.Data.Year.Should().Be(year);
        result.Data.Month.Should().Be(month);
    }

    [Test]
    public async Task ThenResultShouldShowIfTheSelectMonthIsTheLatest()
    {
        //Arrange
        _currentTime.Setup(x => x.Now).Returns(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
        SetupGetTransactionsResponse(DateTime.Now.Year, DateTime.Now.Month);

        //Act
        var resultLatestMonth = await _orchestrator.GetAccountTransactions(HashedAccountId, DateTime.Now.Year, DateTime.Now.Month);
        SetupGetTransactionsResponse(2016, 1);
        var resultHistoricalMonth = await _orchestrator.GetAccountTransactions(HashedAccountId, 2016, 1);

        //Assert
        resultLatestMonth.Data.IsLatestMonth.Should().Be(true);
        resultHistoricalMonth.Data.IsLatestMonth.Should().Be(false);
    }

    [Test]
    public async Task ThenResultShouldHaveWhetherPreviousTransactionsAreAvailable()
    {
        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, 2017, 8);

        //Assert
        result.Data.AccountHasPreviousTransactions.Should().BeTrue();
    }

    [Test]
    public async Task ThenOnlyOneLevyTransactionShouldBeShownForSummary()
    {
        //Arrange
        var levyTransactions = new List<LevyDeclarationTransactionLine>
        {
            CreateLevyTransaction(new DateTime(2017,5,18), 200),
            CreateLevyTransaction(new DateTime(2017,6,18), 300),
            CreateLevyTransaction(new DateTime(2017,7,18), 500)
        };

        var transactions = new List<TransactionLine>();

        transactions.AddRange(levyTransactions);
        transactions.Add(new PaymentTransactionLine { Amount = 200, TransactionType = TransactionItemType.Payment });

        SetupGetTransactionsResponse(2018, 2, transactions);

        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, default(int), default(int));

        var actualTransactions = result?.Data?.Model?.Data?.TransactionLines;

        var levyDeclaration =
            actualTransactions?.SingleOrDefault(t => t.TransactionType == TransactionItemType.Declaration);

        //Assert
        levyDeclaration.Should().NotBeNull();
    }

    [Test]
    public async Task ThenLevyAggregationShouldNotAffectOtherTransactions()
    {
        //Arrange
        var levyTransactions = new List<LevyDeclarationTransactionLine>
        {
            CreateLevyTransaction(new DateTime(2017,5,18), 200),
            CreateLevyTransaction(new DateTime(2017,6,18), 300),
            CreateLevyTransaction(new DateTime(2017,7,18), 500)
        };

        var transactions = new List<TransactionLine>();

        transactions.AddRange(levyTransactions);
        transactions.Add(new PaymentTransactionLine { Amount = 200, TransactionType = TransactionItemType.Payment });

        SetupGetTransactionsResponse(2018, 2, transactions);

        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, default(int), default(int));

        var actualTransactions = result?.Data?.Model?.Data?.TransactionLines;

        var paymentTransaction =
            actualTransactions?.SingleOrDefault(t => t.TransactionType == TransactionItemType.Payment);

        //Assert
        paymentTransaction.Should().NotBeNull();
        actualTransactions.Length.Should().Be(2);
    }

    [Test]
    public async Task ThenAggregatedLevyTransactionShouldHaveCorrectDescription()
    {
        //Arrange
        var levyTransactions = new List<LevyDeclarationTransactionLine>
        {
            CreateLevyTransaction(new DateTime(2017,5,18), 200),
            CreateLevyTransaction(new DateTime(2017,6,18), 300),
            CreateLevyTransaction(new DateTime(2017,7,18), 500)
        };

        var transactions = new List<TransactionLine>();

        transactions.AddRange(levyTransactions);
        transactions.Add(new PaymentTransactionLine { Amount = 200, TransactionType = TransactionItemType.Payment });

        SetupGetTransactionsResponse(2018, 2, transactions);

        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, default(int), default(int));

        var actualTransactions = result?.Data?.Model?.Data?.TransactionLines;

        var levyDeclaration =
            actualTransactions?.SingleOrDefault(t => t.TransactionType == TransactionItemType.Declaration);

        //Assert
        levyDeclaration?.Description.Should().Be(levyTransactions.First().Description);
    }

    [Test]
    public async Task ThenAggregatedLevyTransactionShouldHaveCorrectAmount()
    {
        //Arrange           
        var levyTransactions = new List<LevyDeclarationTransactionLine>
        {
            CreateLevyTransaction(new DateTime(2017,5,18), 200),
            CreateLevyTransaction(new DateTime(2017,6,18), 300),
            CreateLevyTransaction(new DateTime(2017,7,18), 500)
        };

        var transactions = new List<TransactionLine>();

        transactions.AddRange(levyTransactions);
        transactions.Add(new PaymentTransactionLine { Amount = 200, TransactionType = TransactionItemType.Payment });

        SetupGetTransactionsResponse(2018, 2, transactions);

        //Act
        var result = await _orchestrator.GetAccountTransactions(HashedAccountId, default(int), default(int));

        var actualTransactions = result?.Data?.Model?.Data?.TransactionLines;

        //Assert
        actualTransactions.Should().NotBeNull();
        actualTransactions.Single(t => t.TransactionType == TransactionItemType.Declaration).Amount.Should().Be(levyTransactions.Sum(t => t.Amount));
    }

    private void SetupGetTransactionsResponse(int year, int month)
    {
        SetupGetTransactionsResponse(year, month, Array.Empty<TransactionLine>());
    }

    private void SetupGetTransactionsResponse(int year, int month, IEnumerable<TransactionLine> transactions)
    {
        _accountApiClient.Setup(s => s.GetAccount(HashedAccountId))
            .Returns(Task.FromResult(
                new EAS.Account.Api.Types.AccountDetailViewModel
                {
                    HashedAccountId = HashedAccountId,
                    AccountId = AccountId
                })
            );

        _mediator.Setup(x => x.Send(It.IsAny<GetEmployerAccountTransactionsQuery>(), CancellationToken.None))
            .ReturnsAsync(new GetEmployerAccountTransactionsResponse
            {
                Data = new AggregationData
                {
                    TransactionLines = transactions.ToArray()
                },
                Year = year,
                Month = month,
                AccountHasPreviousTransactions = true
            });
    }

    private static LevyDeclarationTransactionLine CreateLevyTransaction(DateTime submissionDate, int amount)
    {
        return new LevyDeclarationTransactionLine
        {
            Amount = amount,
            DateCreated = DateTime.Now,
            TransactionDate = submissionDate,
            TransactionType = TransactionItemType.Declaration,
            Description = "Levy"
        };
    }
}