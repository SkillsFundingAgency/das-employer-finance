using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Transaction;

namespace SFA.DAS.EmployerFinance.UnitTests.Data;

public class FinanceDashboardRepositoryEfTests
{
    private const long AccountId = 100;
    private static readonly DateTime FromDate = new DateTime(2024, 1, 1);
    private static readonly DateTime ToDate = new DateTime(2024, 1, 31);

    private static EmployerFinanceDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployerFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EmployerFinanceDbContext(options);
    }

    [Test]
    public async Task GetAccountBalanceAsync_ReturnsSumOfEligibleTransactionTypes()
    {
        // Arrange
        await using var context = CreateContext();
        context.Transactions.AddRange(
            new TransactionLineEntity
            {
                Id = 1, AccountId = AccountId, TransactionType = TransactionItemType.Declaration,
                Amount = 100m, TransactionDate = DateTime.UtcNow, DateCreated = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 2, AccountId = AccountId, TransactionType = TransactionItemType.TopUp,
                Amount = 50m, TransactionDate = DateTime.UtcNow, DateCreated = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 3, AccountId = AccountId, TransactionType = TransactionItemType.Payment,
                Amount = -30m, TransactionDate = DateTime.UtcNow, DateCreated = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 4, AccountId = AccountId, TransactionType = TransactionItemType.Unknown,
                Amount = 999m, TransactionDate = DateTime.UtcNow, DateCreated = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetAccountBalanceAsync(AccountId);

        // Assert
        result.Should().Be(120m);
    }

    [Test]
    public async Task GetAccountBalanceAsync_ReturnsZeroForAccountWithNoTransactions()
    {
        // Arrange
        await using var context = CreateContext();
        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetAccountBalanceAsync(AccountId);

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public async Task GetAccountBalanceAsync_ExcludesOtherAccounts()
    {
        // Arrange
        await using var context = CreateContext();
        context.Transactions.Add(
            new TransactionLineEntity
            {
                Id = 1, AccountId = AccountId + 1, TransactionType = TransactionItemType.Declaration,
                Amount = 500m, TransactionDate = DateTime.UtcNow, DateCreated = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetAccountBalanceAsync(AccountId);

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public async Task GetTotalSpendForLastYearAsync_ReturnsSumOfPaymentAndTransferInLastYear()
    {
        // Arrange
        await using var context = CreateContext();
        var oneYearAgo = DateTime.UtcNow.AddYears(-1).Date;
        context.Transactions.AddRange(
            new TransactionLineEntity
            {
                Id = 1, AccountId = AccountId, TransactionType = TransactionItemType.Payment,
                Amount = -100m, TransactionDate = oneYearAgo.AddDays(1), DateCreated = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 2, AccountId = AccountId, TransactionType = TransactionItemType.Transfer,
                Amount = -50m, TransactionDate = DateTime.UtcNow.AddDays(-1), DateCreated = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 3, AccountId = AccountId, TransactionType = TransactionItemType.Declaration,
                Amount = -200m, TransactionDate = oneYearAgo.AddDays(1), DateCreated = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetTotalSpendForLastYearAsync(AccountId);

        // Assert
        result.Should().Be(-150m);
    }

    [Test]
    public async Task GetTotalSpendForLastYearAsync_ReturnsZeroWhenNoPaymentOrTransfer()
    {
        // Arrange
        await using var context = CreateContext();
        var oneYearAgo = DateTime.UtcNow.AddYears(-1).Date;
        context.Transactions.Add(
            new TransactionLineEntity
            {
                Id = 1, AccountId = AccountId, TransactionType = TransactionItemType.Declaration,
                Amount = 100m, TransactionDate = oneYearAgo.AddDays(1), DateCreated = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetTotalSpendForLastYearAsync(AccountId);

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public async Task GetLastMonthPaymentsAndTransfersAsync_ReturnsSumInDateRange()
    {
        // Arrange
        await using var context = CreateContext();
        context.Transactions.AddRange(
            new TransactionLineEntity
            {
                Id = 1, AccountId = AccountId, TransactionType = TransactionItemType.Payment,
                Amount = -80m, DateCreated = FromDate.AddDays(5), TransactionDate = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 2, AccountId = AccountId, TransactionType = TransactionItemType.Transfer,
                Amount = -20m, DateCreated = ToDate.AddDays(-1), TransactionDate = DateTime.UtcNow
            },
            new TransactionLineEntity
            {
                Id = 3, AccountId = AccountId, TransactionType = TransactionItemType.Payment,
                Amount = -100m, DateCreated = FromDate.AddDays(-1), TransactionDate = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetLastMonthPaymentsAndTransfersAsync(AccountId, FromDate, ToDate);

        // Assert
        result.Should().Be(-100m);
    }

    [Test]
    public async Task GetLastMonthPaymentsAndTransfersAsync_ReturnsZeroWhenNoDataInRange()
    {
        // Arrange
        await using var context = CreateContext();
        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var result = await sut.GetLastMonthPaymentsAndTransfersAsync(AccountId, FromDate, ToDate);

        // Assert
        result.Should().Be(0m);
    }

    [Test]
    public void GetLatestLevyDeclarationTotalAsync_ThrowsWhenUsingInMemoryDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<EmployerFinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        using var context = new EmployerFinanceDbContext(options);
        var logger = new Mock<ILogger<FinanceDashboardRepositoryEf>>();
        var sut = new FinanceDashboardRepositoryEf(context, logger.Object);

        // Act
        var act = () => sut.GetLatestLevyDeclarationTotalAsync(AccountId);

        // Assert
        act.Should().ThrowAsync<Exception>();
    }
}
