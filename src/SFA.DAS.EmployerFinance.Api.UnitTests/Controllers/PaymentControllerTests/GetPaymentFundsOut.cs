using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetPaymentFundsOutByAccountId;
using SFA.DAS.EmployerFinance.Types.Models;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class GetPaymentFundsOut
{
    private const long AccountId = 12345;

    private PaymentsController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _controller = new PaymentsController(null!, new PaymentOrchestrator(_mediator.Object));
    }

    [Test]
    public async Task Then_Returns_Ok_With_Funds_Out_By_Calendar_Period()
    {
        _mediator
            .Setup(m => m.Send(It.Is<GetPaymentFundsOutByAccountIdQuery>(q => q.AccountId == AccountId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentFundsOutByAccountIdQueryResult(new Dictionary<CalendarPeriod, decimal>
            {
                { new CalendarPeriod(2026, 4), 300 }
            }));

        var result = await _controller.GetPaymentFundsOutForCalendarPeriods(AccountId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new List<CalendarPeriodAmount>
        {
            new() { Year = 2026, Month = 4, Amount = 300 }
        });
    }

    [Test]
    public async Task Then_Returns_Ok_With_An_Empty_List_When_There_Are_No_Payments()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetPaymentFundsOutByAccountIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentFundsOutByAccountIdQueryResult(new Dictionary<CalendarPeriod, decimal>()));

        var result = await _controller.GetPaymentFundsOutForCalendarPeriods(AccountId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<CalendarPeriodAmount>>().Which.Should().BeEmpty();
    }
}
