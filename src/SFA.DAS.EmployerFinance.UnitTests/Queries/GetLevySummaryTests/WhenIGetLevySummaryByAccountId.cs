using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

[TestFixture]
public class WhenIGetLevySummaryByAccountId
{
    private Mock<IDasLevyService> _dasLevyService;
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private Mock<IEncodingService> _encodingService;
    private GetLevySummaryByHashedAccountIdQueryHandler _handler;

    private const string ExpectedHashedAccountId = "ABC123";
    private const long ExpectedAccountId = 99887;
    private const decimal ExpectedAccountBalance = 5000.75m;
    private const decimal ExpectedTotalLevyDeclaredLast12Months = 4500.00m;
    private const decimal ExpectedTotalLevySpentLast12Months = 3000.00m;

    [SetUp]
    public void Arrange()
    {
        _encodingService = new Mock<IEncodingService>();
        _encodingService
            .Setup(x => x.Decode(ExpectedHashedAccountId, EncodingType.AccountId))
            .Returns(ExpectedAccountId);

        _dasLevyService = new Mock<IDasLevyService>();
        _dasLevyService
            .Setup(x => x.GetAccountBalance(ExpectedAccountId))
            .ReturnsAsync(ExpectedAccountBalance);

        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclaredForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem {TotalAmount = 1000m},
                new LevyDeclarationItem {TotalAmount = 2000m},
                new LevyDeclarationItem {TotalAmount = 1500m}
            ]);

        _dasLevyRepository.Setup(x => x.GetAccountLevySpentForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem {TotalAmount = 1000m},
                new LevyDeclarationItem {TotalAmount = 2000m}
            ]);

        _handler = new GetLevySummaryByHashedAccountIdQueryHandler(_dasLevyService.Object, _dasLevyRepository.Object, _encodingService.Object);
    }

    [Test]
    public async Task ThenTheLevyServiceIsCalledWithTheDecodedAccountId()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetAccountBalance(ExpectedAccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheEncodingServiceIsCalledWithTheHashedAccountId()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _encodingService.Verify(x => x.Decode(ExpectedHashedAccountId, EncodingType.AccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheResponseIsNotNull()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheResponseContainsTheLevySummary()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheCurrentLevyFundsIsSetToTheAccountBalance()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.CurrentLevyFunds.Should().Be(ExpectedAccountBalance);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevyFundsIsSetToTheSumOfAllLevyDeclarations()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevySpentFundsIsSetToTheSumOfAllLevySpent()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(ExpectedTotalLevySpentLast12Months);
    }

    [Test]
    public async Task ThenWhenTheAccountBalanceIsZeroItIsReflectedInTheSummary()
    {
        //Arrange
        _dasLevyService
            .Setup(x => x.GetAccountBalance(ExpectedAccountId))
            .ReturnsAsync(0m);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.CurrentLevyFunds.Should().Be(0m);
    }

    [Test]
    public async Task ThenWhenThereAreNoLevyDeclarationsTotalIsZero()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclaredForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(0m);
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsCalledWithTheDecodedAccountIdAndTwelveMonths()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _dasLevyRepository.Verify(x => x.GetAccountLevyDeclaredForPreviousMonths(ExpectedAccountId, 12), Times.Once);
    }

    [Test]
    public async Task ThenWhenThereAreNegativeLevyDeclarationsTheyAreIncludedInTheTotal()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclaredForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem { TotalAmount = 2000m },
                new LevyDeclarationItem { TotalAmount = -500m }  // end of year adjustment / correction
            ]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(1500m);
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsCalledWithTheDecodedAccountIdAndTwelveMonthsForLevySpent()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        _dasLevyRepository.Verify(x => x.GetAccountLevySpentForPreviousMonths(ExpectedAccountId, 12), Times.Once);
    }

    [Test]
    public async Task ThenWhenThereAreNoLevySpentTransactionsTotalIsZero()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountLevySpentForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(0m);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevySpentIsSetToTheSumOfAllLevySpentTransactions()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(ExpectedTotalLevySpentLast12Months);
    }

    [Test]
    public async Task ThenLevyDeclaredAndLevySpentAreIndependentOfEachOther()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclaredForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(0m);
        result.Summary.TotalLevySpentLast12Months.Should().Be(ExpectedTotalLevySpentLast12Months);
    }

    [Test]
    public async Task ThenWhenLevySpentExceedsLevyDeclaredBothValuesAreStillReturned()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountLevySpentForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem { TotalAmount = 5000m },
            new LevyDeclarationItem { TotalAmount = 2000m }
            ]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByHashedAccountIdQuery(ExpectedHashedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(7000m);
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
    }
}