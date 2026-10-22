using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetLevyFundsInByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.FinanceLevyDeclarationsControllerTests;

[TestFixture]
internal class WhenIGetLevyFundsIn
{
    private const long AccountId = 12345;

    private FinanceLevyDeclarationsController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();

        var orchestrator = new LevyDeclarationOrchestrator(_mediator.Object, Mock.Of<IMapper>(), Mock.Of<ILogger<LevyDeclarationOrchestrator>>());

        _controller = new FinanceLevyDeclarationsController(orchestrator);
    }

    [Test]
    public async Task Then_Returns_Funds_In_Ordered_By_Calendar_Period()
    {
        _mediator
            .Setup(x => x.Send(It.Is<GetLevyFundsInByAccountIdQuery>(q => q.AccountId == AccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyFundsInByAccountIdQueryResult(new Dictionary<CalendarPeriod, decimal>
            {
                { new CalendarPeriod(2026, 5), 1100 },
                { new CalendarPeriod(2026, 4), 1000 }
            }));

        var result = await _controller.GetLevyFundsInForCalendarPeriods(AccountId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<CalendarPeriodAmount>
        {
            new() { Year = 2026, Month = 4, Amount = 1000 },
            new() { Year = 2026, Month = 5, Amount = 1100 }
        }, options => options.WithStrictOrdering());
    }

    [Test]
    public async Task Then_Returns_Ok_With_An_Empty_List_When_There_Are_No_Funds_In()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<GetLevyFundsInByAccountIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetLevyFundsInByAccountIdQueryResult(new Dictionary<CalendarPeriod, decimal>()));

        var result = await _controller.GetLevyFundsInForCalendarPeriods(AccountId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<CalendarPeriodAmount>>().Which.Should().BeEmpty();
    }
}
