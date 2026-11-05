using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;
using FundingSource = SFA.DAS.Provider.Events.Api.Types.FundingSource;
using TransactionType = SFA.DAS.Provider.Events.Api.Types.TransactionType;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class GetCollectionPeriodPayments
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
    public async Task Then_Returns_Ok_With_The_Payment_Totals_For_The_Collection_Period()
    {
        _mediator
            .Setup(m => m.Send(
                It.Is<GetCollectionPeriodPaymentsByAccountIdQuery>(q => q.AccountId == AccountId && q.Year == 2026 && q.Month == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCollectionPeriodPaymentsByAccountIdQueryResult(true,
                [new CollectionPeriodPaymentTotal(FundingSource.Levy, TransactionType.Learning, 250)]));

        var result = await _controller.GetCollectionPeriodPayments(AccountId, 2026, 10, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        var body = ok.Value.Should().BeOfType<CollectionPeriodPaymentsResponse>().Subject;
        body.PeriodEndCompleted.Should().BeTrue();
        body.Payments.Should().ContainSingle().Which.Amount.Should().Be(250);
    }
}
