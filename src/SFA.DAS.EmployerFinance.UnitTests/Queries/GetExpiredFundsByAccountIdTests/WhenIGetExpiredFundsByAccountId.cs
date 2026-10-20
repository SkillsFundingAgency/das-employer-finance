using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.ExpiredFunds;
using SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetExpiredFundsByAccountIdTests;

[TestFixture]
public class WhenIGetExpiredFundsByAccountId
{
    private const long AccountId = 12345;

    private Mock<IExpiredFundsRepository> _expiredFundsRepository;
    private GetExpiredFundsByAccountIdQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _expiredFundsRepository = new Mock<IExpiredFundsRepository>();
        _handler = new GetExpiredFundsByAccountIdQueryHandler(_expiredFundsRepository.Object);
    }

    [Test]
    public async Task Then_Long_And_Short_Term_Expiry_In_The_Same_Period_Are_Returned_Separately()
    {
        _expiredFundsRepository.Setup(x => x.Get(AccountId)).ReturnsAsync(new List<ExpiredFund>
        {
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 5, Amount = -100, TransactionType = 5 },
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 5, Amount = -40, TransactionType = 6 },
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 6, Amount = -25, TransactionType = 6 }
        });

        var result = await _handler.Handle(new GetExpiredFundsByAccountIdQuery(AccountId), CancellationToken.None);

        result.LongTermExpiredFunds.Should().BeEquivalentTo(new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2026, 5), 100 }
        });
        result.ShortTermExpiredFunds.Should().BeEquivalentTo(new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2026, 5), 40 },
            { new CalendarPeriod(2026, 6), 25 }
        });
    }

    [Test]
    public async Task Then_Empty_Dictionaries_Are_Returned_When_Nothing_Has_Expired()
    {
        _expiredFundsRepository.Setup(x => x.Get(AccountId)).ReturnsAsync(new List<ExpiredFund>());

        var result = await _handler.Handle(new GetExpiredFundsByAccountIdQuery(AccountId), CancellationToken.None);

        result.LongTermExpiredFunds.Should().BeEmpty();
        result.ShortTermExpiredFunds.Should().BeEmpty();
    }
}
