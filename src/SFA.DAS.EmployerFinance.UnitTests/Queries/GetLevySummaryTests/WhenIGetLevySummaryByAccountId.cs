using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevySummaryTests;

[TestFixture]
public class WhenIGetLevySummaryByAccountId
{
    private Mock<IDasLevyService> _dasLevyService;
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private GetLevySummaryByAccountIdQueryHandler _handler;

    private const long ExpectedAccountId = 99887;
    private const decimal ExpectedAccountBalance = 5000.75m;
    private const decimal ExpectedTotalLevyDeclaredLast12Months = 4500.00m;
    private const decimal ExpectedTotalLevySpentLast12Months = 3000.00m;
    private const decimal ExpectedTotalLevyExpiredLast12Months = 1500.00m;

    [SetUp]
    public void Arrange()
    {
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

        _dasLevyRepository.Setup(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem {TotalAmount = 500m},
                new LevyDeclarationItem {TotalAmount = 500m},
                new LevyDeclarationItem {TotalAmount = 500m}
            ]); 

        _handler = new GetLevySummaryByAccountIdQueryHandler(_dasLevyService.Object, _dasLevyRepository.Object);
    }

    [Test]
    public async Task ThenTheLevyServiceIsCalledWithTheDecodedAccountId()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        _dasLevyService.Verify(x => x.GetAccountBalance(ExpectedAccountId), Times.Once);
    }

    [Test]
    public async Task ThenTheResponseIsNotNull()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheResponseContainsTheLevySummary()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.Should().NotBeNull();
    }

    [Test]
    public async Task ThenTheCurrentLevyFundsIsSetToTheAccountBalance()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.CurrentLevyFunds.Should().Be(ExpectedAccountBalance);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevyFundsIsSetToTheSumOfAllLevyDeclarations()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevySpentFundsIsSetToTheSumOfAllLevySpent()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(0m);
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsCalledWithTheDecodedAccountIdAndTwelveMonths()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(1500m);
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsCalledWithTheDecodedAccountIdAndTwelveMonthsForLevySpent()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(0m);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevySpentIsSetToTheSumOfAllLevySpentTransactions()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

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
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevySpentLast12Months.Should().Be(7000m);
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
    }

    [Test]
    public async Task ThenTheLevyRepositoryIsCalledWithTheDecodedAccountIdAndTwelveMonthsForLevyExpired()
    {
        //Act
        await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        _dasLevyRepository.Verify(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12), Times.Once);
    }

    [Test]
    public async Task ThenTheTwelveMonthsTotalLevyExpiredIsSetToTheSumOfAllExpiredTransactions()
    {
        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyExpiredLast12Months.Should().Be(ExpectedTotalLevyExpiredLast12Months);
    }

    [Test]
    public async Task ThenWhenThereAreNoExpiredLevyTransactionsTotalIsZero()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyExpiredLast12Months.Should().Be(0m);
    }

    [Test]
    public async Task ThenLevyExpiredIsIndependentOfLevyDeclaredAndLevySpent()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyExpiredLast12Months.Should().Be(0m);
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
        result.Summary.TotalLevySpentLast12Months.Should().Be(ExpectedTotalLevySpentLast12Months);
    }

    [Test]
    public async Task ThenWhenExpiredLevyAmountsAreNegativeTheyAreIncludedInTheTotal()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem { TotalAmount = -500m },
            new LevyDeclarationItem { TotalAmount = -1000m }
            ]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyExpiredLast12Months.Should().Be(-1500m);
    }

    [Test]
    public async Task ThenWhenExpiredLevyExceedsLevyDeclaredBothValuesAreStillReturned()
    {
        //Arrange
        _dasLevyRepository
            .Setup(x => x.GetAccountExpiredLevyForPreviousMonths(ExpectedAccountId, 12))
            .ReturnsAsync([
                new LevyDeclarationItem { TotalAmount = 3000m },
            new LevyDeclarationItem { TotalAmount = 3000m }
            ]);

        //Act
        var result = await _handler.Handle(new GetLevySummaryByAccountIdQuery(ExpectedAccountId), CancellationToken.None);

        //Assert
        result.Summary.TotalLevyExpiredLast12Months.Should().Be(6000m);
        result.Summary.TotalLevyDeclaredLast12Months.Should().Be(ExpectedTotalLevyDeclaredLast12Months);
    }
}