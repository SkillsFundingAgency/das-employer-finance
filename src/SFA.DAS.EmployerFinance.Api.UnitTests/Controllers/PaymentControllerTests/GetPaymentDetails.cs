using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Controllers;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Controllers.PaymentControllerTests;

public class GetPaymentDetails
{
    private const long AccountId = 12345;
    private const long ApprenticeshipId = 678;

    private PaymentsController _controller;
    private Mock<IMediator> _mediator;

    [SetUp]
    public void Arrange()
    {
        _mediator = new Mock<IMediator>();
        _controller = new PaymentsController(null!, new PaymentOrchestrator(_mediator.Object));
    }

    [Test]
    public async Task Then_Returns_Ok_With_All_Payments_For_The_Apprenticeship()
    {
        var payments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), EmployerAccountId = AccountId, ApprenticeshipId = ApprenticeshipId, Amount = 100 },
            new() { Id = Guid.NewGuid(), EmployerAccountId = AccountId, ApprenticeshipId = ApprenticeshipId, Amount = 150 }
        };
        _mediator
            .Setup(m => m.Send(
                It.Is<GetPaymentsByAccountIdAndApprenticeshipIdQuery>(q => q.AccountId == AccountId && q.ApprenticeshipId == ApprenticeshipId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentsByAccountIdAndApprenticeshipIdQueryResult(payments));

        var result = await _controller.GetPaymentDetails(AccountId, ApprenticeshipId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        var body = ok.Value.Should().BeAssignableTo<IEnumerable<PaymentResponse>>().Subject.ToList();
        body.Select(p => p.Id).Should().BeEquivalentTo(payments.Select(p => p.Id));
        body.Sum(p => p.Amount).Should().Be(250);
    }

    [Test]
    public async Task Then_Returns_Ok_With_An_Empty_List_When_There_Are_No_Payments()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetPaymentsByAccountIdAndApprenticeshipIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentsByAccountIdAndApprenticeshipIdQueryResult([]));

        var result = await _controller.GetPaymentDetails(AccountId, ApprenticeshipId, CancellationToken.None);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<PaymentResponse>>().Which.Should().BeEmpty();
    }
}