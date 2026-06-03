using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.FindEmployerAccountExpiredFundsTests;

public class WhenIGetEmployerExpiredFunds : QueryBaseTest<FindEmployerAccountExpiredFundsQueryHandler, FindEmployerAccountExpiredFundsQuery, FindEmployerAccountExpiredFundsResponse>
{
    private Mock<IDasLevyService> _dasLevyService;
    private Mock<IEncodingService> _encodingService;
    private DateTime _fromDate;
    private DateTime _toDate;
    private long _accountId;
    private string _hashedAccountId;
    private DateTime _transactionDate;

    public override FindEmployerAccountExpiredFundsQuery Query { get; set; }
    public override FindEmployerAccountExpiredFundsQueryHandler RequestHandler { get; set; }
    public override Mock<IValidator<FindEmployerAccountExpiredFundsQuery>> RequestValidator { get; set; }

    [SetUp]
    public void Arrange()
    {
        SetUp();

        _fromDate = DateTime.Now.AddDays(-10);
        _toDate = DateTime.Now.AddDays(-2);
        _accountId = 1;
        _hashedAccountId = "123ABC";
        _transactionDate = DateTime.Now.AddDays(-5);

        _encodingService = new Mock<IEncodingService>();
        _encodingService.Setup(x => x.Decode(It.IsAny<string>(), EncodingType.AccountId)).Returns(_accountId);

        _dasLevyService = new Mock<IDasLevyService>();
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[]
            {
                new TransactionLine { TransactionType = TransactionItemType.ExpiredFund, Amount = 100m, TransactionDate = _transactionDate },
                new TransactionLine { TransactionType = TransactionItemType.ShortExpiredFund, Amount = 50m, TransactionDate = _transactionDate }
            });

        Query = new FindEmployerAccountExpiredFundsQuery
        {
            HashedAccountId = _hashedAccountId,
            FromDate = _fromDate,
            ToDate = _toDate
        };

        RequestHandler = new FindEmployerAccountExpiredFundsQueryHandler(
            RequestValidator.Object,
            _dasLevyService.Object,
            _encodingService.Object);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
    {
        //Act
        await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        _encodingService.Verify(x => x.Decode(_hashedAccountId, EncodingType.AccountId), Times.Once);
        _dasLevyService.Verify(x => x.GetAccountTransactionsByDateRange(_accountId, _fromDate, _toDate), Times.Once);
    }

    [Test]
    public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Should().NotBeNull();
        actual.Total.Should().NotBe(0);
    }

    [Test]
    public async Task ThenTheTotalIsTheSumOfAllExpiredFundTransactions()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Total.Should().Be(150m);
    }

    [Test]
    public async Task ThenTheTwentyFourthMonthExpiryAmountIsSumOfExpiredFundTransactions()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.TwentyFourthMonthExpiryAmount.Should().Be(100m);
    }

    [Test]
    public async Task ThenTheTwelveMonthExpiryAmountIsSumOfShortExpiredFundTransactions()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.TwelveMonthExpiryAmount.Should().Be(50m);
    }

    [Test]
    public async Task ThenTheTransactionDateIsFromTheFirstTransaction()
    {
        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.TransactionDate.Should().Be(_transactionDate);
    }

    [Test]
    public async Task ThenNonExpiredFundTransactionsAreExcludedFromTotals()
    {
        //Arrange
        _dasLevyService.Setup(x => x.GetAccountTransactionsByDateRange(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new[]
            {
                new TransactionLine { TransactionType = TransactionItemType.ExpiredFund, Amount = 100m, TransactionDate = _transactionDate },
                new TransactionLine { TransactionType = TransactionItemType.Declaration, Amount = 999m, TransactionDate = _transactionDate },
                new TransactionLine { TransactionType = TransactionItemType.Payment, Amount = 999m, TransactionDate = _transactionDate }
            });

        //Act
        var actual = await RequestHandler.Handle(Query, CancellationToken.None);

        //Assert
        actual.Total.Should().Be(100m);
    }
}
