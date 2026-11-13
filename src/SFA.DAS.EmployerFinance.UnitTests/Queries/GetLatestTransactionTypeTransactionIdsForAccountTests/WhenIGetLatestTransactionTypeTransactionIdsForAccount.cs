using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Queries.GetLatestTransactionTypeTransactionIdsForAccount;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLatestTransactionTypeTransactionIdsForAccountTests;

[TestFixture]
public class WhenIGetLatestTransactionTypeTransactionIdsForAccount
{
    private const long AccountId = 12345;

    private Mock<ITransactionRepository> _transactionRepository;
    private GetLatestTransactionTypeTransactionIdsForAccountQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _transactionRepository = new Mock<ITransactionRepository>();
        _handler = new GetLatestTransactionTypeTransactionIdsForAccountQueryHandler(_transactionRepository.Object);
    }

    [Test]
    public async Task Then_The_Highest_Id_Of_Each_Group_Of_Transaction_Types_Is_Returned()
    {
        _transactionRepository.Setup(x => x.GetLatestTransactionTypeTransactionIdsForAccount(AccountId)).ReturnsAsync(new List<LatestTransactionTypeTransactionId>
        {
            new() { TransactionType = TransactionItemType.Declaration, LatestId = 120 },
            new() { TransactionType = TransactionItemType.TopUp, LatestId = 121 },
            new() { TransactionType = TransactionItemType.Payment, LatestId = 115 },
            new() { TransactionType = TransactionItemType.Transfer, LatestId = 110 },
            new() { TransactionType = TransactionItemType.ExpiredFund, LatestId = 90 },
            new() { TransactionType = TransactionItemType.ShortExpiredFund, LatestId = 100 }
        });

        var result = await _handler.Handle(new GetLatestTransactionTypeTransactionIdsForAccountQuery(AccountId), CancellationToken.None);

        result.Should().Be(new GetLatestTransactionTypeTransactionIdsForAccountQueryResult(121, 115, 100));
    }

    [Test]
    public async Task Then_Null_Ids_Are_Returned_When_There_Are_No_Transactions()
    {
        _transactionRepository.Setup(x => x.GetLatestTransactionTypeTransactionIdsForAccount(AccountId)).ReturnsAsync(new List<LatestTransactionTypeTransactionId>());

        var result = await _handler.Handle(new GetLatestTransactionTypeTransactionIdsForAccountQuery(AccountId), CancellationToken.None);

        result.Should().Be(new GetLatestTransactionTypeTransactionIdsForAccountQueryResult(null, null, null));
    }

    [Test]
    public async Task Then_Only_The_Groups_With_Transactions_Have_An_Id()
    {
        _transactionRepository.Setup(x => x.GetLatestTransactionTypeTransactionIdsForAccount(AccountId)).ReturnsAsync(new List<LatestTransactionTypeTransactionId>
        {
            new() { TransactionType = TransactionItemType.Declaration, LatestId = 120 }
        });

        var result = await _handler.Handle(new GetLatestTransactionTypeTransactionIdsForAccountQuery(AccountId), CancellationToken.None);

        result.Should().Be(new GetLatestTransactionTypeTransactionIdsForAccountQueryResult(120, null, null));
    }
}
