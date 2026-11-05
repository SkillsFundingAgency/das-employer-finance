using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetCollectionPeriodPaymentsByAccountId;
using FundingSource = SFA.DAS.Provider.Events.Api.Types.FundingSource;
using TransactionType = SFA.DAS.Provider.Events.Api.Types.TransactionType;
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
        var first = CreatePayment(collectionPeriodMonth: 9, amount: 100, dateImported: new DateTime(2026, 10, 6), FundingSource.Levy, TransactionType.Learning);
        var second = CreatePayment(collectionPeriodMonth: 10, amount: 150, dateImported: new DateTime(2026, 11, 6), FundingSource.CoInvestedEmployer, TransactionType.Completion);

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

    [Test]
    public async Task Then_GetCollectionPeriodPayments_Maps_The_Payment_Totals()
    {
        var mediator = new Mock<IMediator>();
        var orchestrator = new PaymentOrchestrator(mediator.Object);

        mediator
            .Setup(x => x.Send(
                It.Is<GetCollectionPeriodPaymentsByAccountIdQuery>(q => q.AccountId == AccountId && q.Year == 2026 && q.Month == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCollectionPeriodPaymentsByAccountIdQueryResult(true,
            [
                new CollectionPeriodPaymentTotal(FundingSource.Levy, TransactionType.Learning, 250),
                new CollectionPeriodPaymentTotal(FundingSource.LevyTransfer, TransactionType.Completion, 400),
            ]));

        var result = await orchestrator.GetCollectionPeriodPayments(AccountId, 2026, 10, CancellationToken.None);

        result.PeriodEndCompleted.Should().BeTrue();
        result.Payments.Should().BeEquivalentTo(new[]
        {
            new CollectionPeriodPaymentTotalResponse { FundingSource = 1, TransactionType = 1, Amount = 250 },
            new CollectionPeriodPaymentTotalResponse { FundingSource = 5, TransactionType = 2, Amount = 400 },
        });
    }

    [Test]
    public async Task Then_GetCollectionPeriodPayments_Returns_Not_Completed_When_The_Period_End_Has_Not_Run()
    {
        var mediator = new Mock<IMediator>();
        var orchestrator = new PaymentOrchestrator(mediator.Object);

        mediator
            .Setup(x => x.Send(It.IsAny<GetCollectionPeriodPaymentsByAccountIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCollectionPeriodPaymentsByAccountIdQueryResult(false, []));

        var result = await orchestrator.GetCollectionPeriodPayments(AccountId, 2026, 10, CancellationToken.None);

        result.PeriodEndCompleted.Should().BeFalse();
        result.Payments.Should().BeEmpty();
    }

    private static Payment CreatePayment(int collectionPeriodMonth, decimal amount, DateTime dateImported,
        FundingSource fundingSource = FundingSource.Levy, TransactionType transactionType = TransactionType.Learning) => new()
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
        FundingSource = fundingSource,
        TransactionType = transactionType,
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
        FundingSource = (int)payment.FundingSource,
        TransactionType = (int)payment.TransactionType,
        Amount = payment.Amount,
        LastUpdatedDate = payment.DateImported
    };
}