using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetExpiredFundsByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.ExpiryLevyControllerTests;

[TestFixture]
public class WhenIGetExpiredFunds
{
    private const long AccountId = 12345;

    private ExpiredLevyController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _controller = new ExpiredLevyController(new ExpiredLevyOrchestrator(_mediator.Object, Mock.Of<ILogger<ExpiredLevyOrchestrator>>()));
    }

    [Test]
    public async Task Then_Returns_Long_And_Short_Term_Expired_Funds()
    {
        _mediator
            .Setup(m => m.Send(It.Is<GetExpiredFundsByAccountIdQuery>(q => q.AccountId == AccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetExpiredFundsByAccountIdQueryResult(
                new Dictionary<CalendarPeriod, decimal> { { new CalendarPeriod(2026, 5), 100 } },
                new Dictionary<CalendarPeriod, decimal> { { new CalendarPeriod(2026, 5), 40 } }));

        var result = await _controller.GetExpiredFunds(AccountId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new ExpiredLevyResponse
        {
            LongTerm = [new CalendarPeriodAmount { Year = 2026, Month = 5, Amount = 100 }],
            ShortTerm = [new CalendarPeriodAmount { Year = 2026, Month = 5, Amount = 40 }]
        });
    }

    [Test]
    public async Task Then_Returns_Ok_With_Empty_Lists_When_Nothing_Has_Expired()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetExpiredFundsByAccountIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetExpiredFundsByAccountIdQueryResult(
                new Dictionary<CalendarPeriod, decimal>(),
                new Dictionary<CalendarPeriod, decimal>()));

        var result = await _controller.GetExpiredFunds(AccountId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ExpiredLevyResponse>().Subject;
        body.LongTerm.Should().NotBeNull().And.BeEmpty();
        body.ShortTerm.Should().NotBeNull().And.BeEmpty();
    }
}
