using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPaymentsByAccountIdAndApprenticeshipId;

namespace SFA.DAS.EmployerFinance.Api.UnitTests.Orchestrators;

public class PaymentOrchestratorTests
{
    private const long AccountId = 12345;
    private const long ApprenticeshipId = 678;

    [Test]
    public async Task Then_GetPaymentDetails_Maps_Every_Payment()
    {
        var mediator = new Mock<IMediator>();
        var orchestrator = new PaymentOrchestrator(mediator.Object);
        var first = CreatePayment(collectionPeriodMonth: 9, amount: 100, dateImported: new DateTime(2026, 10, 6));
        var second = CreatePayment(collectionPeriodMonth: 10, amount: 150, dateImported: new DateTime(2026, 11, 6));

        mediator
            .Setup(x => x.Send(
                It.Is<GetPaymentsByAccountIdAndApprenticeshipIdQuery>(q => q.AccountId == AccountId && q.ApprenticeshipId == ApprenticeshipId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentsByAccountIdAndApprenticeshipIdQueryResult([first, second]));

        var result = (await orchestrator.GetPaymentDetails(AccountId, ApprenticeshipId, CancellationToken.None)).ToList();

        result.Should().HaveCount(2);
        result.Should().ContainEquivalentOf(ToExpectedResponse(first));
        result.Should().ContainEquivalentOf(ToExpectedResponse(second));
    }

    [Test]
    public async Task Then_GetPaymentDetails_Returns_Empty_When_There_Are_No_Payments()
    {
        var mediator = new Mock<IMediator>();
        var orchestrator = new PaymentOrchestrator(mediator.Object);

        mediator
            .Setup(x => x.Send(It.IsAny<GetPaymentsByAccountIdAndApprenticeshipIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPaymentsByAccountIdAndApprenticeshipIdQueryResult([]));

        var result = await orchestrator.GetPaymentDetails(AccountId, ApprenticeshipId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    private static Payment CreatePayment(int collectionPeriodMonth, decimal amount, DateTime dateImported) => new()
    {
        Id = Guid.NewGuid(),
        Ukprn = 10000001,
        Uln = 1234567890,
        EmployerAccountId = AccountId,
        ApprenticeshipId = ApprenticeshipId,
        DeliveryPeriodMonth = collectionPeriodMonth - 1,
        DeliveryPeriodYear = 2026,
        CollectionPeriodMonth = collectionPeriodMonth,
        CollectionPeriodYear = 2026,
        Amount = amount,
        DateImported = dateImported
    };

    private static PaymentResponse ToExpectedResponse(Payment payment) => new()
    {
        Id = payment.Id,
        Ukprn = payment.Ukprn,
        Uln = payment.Uln,
        EmployerAccountId = payment.EmployerAccountId,
        ApprenticeshipId = payment.ApprenticeshipId,
        DeliveryPeriodMonth = payment.DeliveryPeriodMonth,
        DeliveryPeriodYear = payment.DeliveryPeriodYear,
        CollectionPeriodMonth = payment.CollectionPeriodMonth,
        CollectionPeriodYear = payment.CollectionPeriodYear,
        Amount = payment.Amount,
        LastUpdatedDate = payment.DateImported
    };
}