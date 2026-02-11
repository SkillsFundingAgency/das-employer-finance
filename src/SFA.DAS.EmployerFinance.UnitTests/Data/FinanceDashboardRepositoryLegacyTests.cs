using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Services.Contracts;

namespace SFA.DAS.EmployerFinance.UnitTests.Data;

public class FinanceDashboardRepositoryLegacyTests
{
    private const long AccountId = 100;
    private static readonly DateTime FromDate = new DateTime(2024, 1, 1);
    private static readonly DateTime ToDate = new DateTime(2024, 1, 31);

    private Mock<ITransactionRepository> _transactionRepository;
    private Mock<IDasLevyService> _levyService;
    private Mock<IDasLevyRepository> _dasLevyRepository;
    private Mock<ILogger<FinanceDashboardRepositoryLegacy>> _logger;
    private FinanceDashboardRepositoryLegacy _sut;

    [SetUp]
    public void Setup()
    {
        _transactionRepository = new Mock<ITransactionRepository>();
        _levyService = new Mock<IDasLevyService>();
        _dasLevyRepository = new Mock<IDasLevyRepository>();
        _logger = new Mock<ILogger<FinanceDashboardRepositoryLegacy>>();
        _sut = new FinanceDashboardRepositoryLegacy(
            _transactionRepository.Object,
            _levyService.Object,
            _dasLevyRepository.Object,
            _logger.Object);
    }

    [Test]
    public async Task GetAccountBalanceAsync_ReturnsValueFromTransactionRepository()
    {
        // Arrange
        const decimal expected = 12345.67m;
        _transactionRepository
            .Setup(x => x.GetAccountBalance(AccountId))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetAccountBalanceAsync(AccountId);

        // Assert
        result.Should().Be(expected);
        _transactionRepository.Verify(x => x.GetAccountBalance(AccountId), Times.Once);
    }

    [Test]
    public async Task GetAccountBalanceAsync_PropagatesException()
    {
        // Arrange
        var ex = new InvalidOperationException("Db error");
        _transactionRepository
            .Setup(x => x.GetAccountBalance(AccountId))
            .ThrowsAsync(ex);

        // Act
        var act = () => _sut.GetAccountBalanceAsync(AccountId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Db error");
    }

    [Test]
    public async Task GetTotalSpendForLastYearAsync_ReturnsValueFromTransactionRepository()
    {
        // Arrange
        const decimal expected = 5000m;
        _transactionRepository
            .Setup(x => x.GetTotalSpendForLastYear(AccountId))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetTotalSpendForLastYearAsync(AccountId);

        // Assert
        result.Should().Be(expected);
        _transactionRepository.Verify(x => x.GetTotalSpendForLastYear(AccountId), Times.Once);
    }

    [Test]
    public async Task GetTotalSpendForLastYearAsync_PropagatesException()
    {
        // Arrange
        var ex = new InvalidOperationException("Db error");
        _transactionRepository
            .Setup(x => x.GetTotalSpendForLastYear(AccountId))
            .ThrowsAsync(ex);

        // Act
        var act = () => _sut.GetTotalSpendForLastYearAsync(AccountId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task GetLastMonthPaymentsAndTransfersAsync_SumsPaymentAndTransferAmounts()
    {
        // Arrange
        var transactionLines = new[]
        {
            new TransactionLine { TransactionType = TransactionItemType.Payment, Amount = -100m },
            new TransactionLine { TransactionType = TransactionItemType.Transfer, Amount = -50m },
            new TransactionLine { TransactionType = TransactionItemType.Declaration, Amount = 200m }
        };
        _levyService
            .Setup(x => x.GetAccountTransactionsByDateRange(AccountId, FromDate, ToDate))
            .ReturnsAsync(transactionLines);

        // Act
        var result = await _sut.GetLastMonthPaymentsAndTransfersAsync(AccountId, FromDate, ToDate);

        // Assert
        result.Should().Be(-150m);
        _levyService.Verify(
            x => x.GetAccountTransactionsByDateRange(AccountId, FromDate, ToDate),
            Times.Once);
    }

    [Test]
    public async Task GetLastMonthPaymentsAndTransfersAsync_ReturnsZeroWhenNoPaymentOrTransfer()
    {
        // Arrange
        var transactionLines = new[]
        {
            new TransactionLine { TransactionType = TransactionItemType.Declaration, Amount = 200m }
        };
        _levyService
            .Setup(x => x.GetAccountTransactionsByDateRange(AccountId, FromDate, ToDate))
            .ReturnsAsync(transactionLines);

        // Act
        var result = await _sut.GetLastMonthPaymentsAndTransfersAsync(AccountId, FromDate, ToDate);

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public async Task GetLastMonthPaymentsAndTransfersAsync_PropagatesException()
    {
        // Arrange
        var ex = new InvalidOperationException("Db error");
        _levyService
            .Setup(x => x.GetAccountTransactionsByDateRange(AccountId, FromDate, ToDate))
            .ThrowsAsync(ex);

        // Act
        var act = () => _sut.GetLastMonthPaymentsAndTransfersAsync(AccountId, FromDate, ToDate);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task GetLatestLevyDeclarationTotalAsync_ReturnsValueFromLevyService()
    {
        // Arrange
        const decimal expected = 10000m;
        _levyService
            .Setup(x => x.GetLatestLevyDeclaration(AccountId))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetLatestLevyDeclarationTotalAsync(AccountId);

        // Assert
        result.Should().Be(expected);
        _levyService.Verify(x => x.GetLatestLevyDeclaration(AccountId), Times.Once);
    }

    [Test]
    public async Task GetLatestLevyDeclarationTotalAsync_PropagatesException()
    {
        // Arrange
        var ex = new InvalidOperationException("Db error");
        _levyService
            .Setup(x => x.GetLatestLevyDeclaration(AccountId))
            .ThrowsAsync(ex);

        // Act
        var act = () => _sut.GetLatestLevyDeclarationTotalAsync(AccountId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task GetLevyDeclarationTotalForMonthAsync_ReturnsSumOfTotalAmountForMonth()
    {
        // Arrange
        const string payrollYear = "24-25";
        const int payrollMonth = 7;
        var declarations = new List<LevyDeclarationItem>
        {
            new() { TotalAmount = 100m },
            new() { TotalAmount = 250m }
        };
        _dasLevyRepository
            .Setup(x => x.GetAccountLevyDeclarations(AccountId, payrollYear, (short)payrollMonth))
            .ReturnsAsync(declarations);

        // Act
        var result = await _sut.GetLevyDeclarationTotalForMonthAsync(AccountId, payrollYear, payrollMonth);

        // Assert
        result.Should().Be(350m);
        _dasLevyRepository.Verify(x => x.GetAccountLevyDeclarations(AccountId, payrollYear, (short)payrollMonth), Times.Once);
    }
}
