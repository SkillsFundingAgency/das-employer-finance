using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Levy;
using SFA.DAS.EmployerFinance.Queries.GetLevyFundsInByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.UnitTests.Queries.GetLevyFundsInByAccountIdTests;

[TestFixture]
public class WhenIGetLevyFundsInByAccountId
{
    private const long AccountId = 12345;

    private Mock<ILevyFundsInRepository> _levyFundsInRepository;
    private GetLevyFundsInByAccountIdQueryHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _levyFundsInRepository = new Mock<ILevyFundsInRepository>();
        _handler = new GetLevyFundsInByAccountIdQueryHandler(_levyFundsInRepository.Object);
    }

    [Test]
    public async Task Then_Funds_In_Are_Returned_By_Calendar_Period()
    {
        _levyFundsInRepository.Setup(x => x.GetLevyFundsIn(AccountId)).ReturnsAsync(new List<LevyFundsIn>
        {
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 4, FundsIn = 1000 },
            new() { CalendarPeriodYear = 2026, CalendarPeriodMonth = 5, FundsIn = 1100 }
        });

        var result = await _handler.Handle(new GetLevyFundsInByAccountIdQuery(AccountId), CancellationToken.None);

        result.FundsIn.Should().BeEquivalentTo(new Dictionary<CalendarPeriod, decimal>
        {
            { new CalendarPeriod(2026, 4), 1000 },
            { new CalendarPeriod(2026, 5), 1100 }
        });
    }

    [Test]
    public async Task Then_An_Empty_Dictionary_Is_Returned_When_There_Are_No_Funds_In()
    {
        _levyFundsInRepository.Setup(x => x.GetLevyFundsIn(AccountId)).ReturnsAsync(new List<LevyFundsIn>());

        var result = await _handler.Handle(new GetLevyFundsInByAccountIdQuery(AccountId), CancellationToken.None);

        result.FundsIn.Should().NotBeNull().And.BeEmpty();
    }
}
