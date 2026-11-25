using FluentAssertions;
using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.BulkPaymentsIngestTests;

public class WhenValidatingBulkPaymentsIngest
{
    private readonly BulkPaymentsIngestCommandValidator _validator = new();

    [Test]
    public void ThenIsValidWhenAllMandatoryFieldsArePresent()
    {
        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [CreateValidPayment()]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenAllowsNegativeAmount()
    {
        var payment = CreateValidPayment();
        payment.Amount = -100m;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenAllowsZeroAmount()
    {
        var payment = CreateValidPayment();
        payment.Amount = 0m;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenAllowsZeroUkprnUlnAndApprenticeshipId()
    {
        var payment = CreateValidPayment();
        payment.Ukprn = 0;
        payment.Uln = 0;
        payment.ApprenticeshipId = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenAllowsZeroDeliveryAndCollectionPeriodValues()
    {
        var payment = CreateValidPayment();
        payment.DeliveryPeriodMonth = 0;
        payment.DeliveryPeriodYear = 0;
        payment.CollectionPeriodMonth = 0;
        payment.CollectionPeriodYear = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeTrue();
    }

    [Test]
    public void ThenRejectsEmptyPaymentId()
    {
        var payment = CreateValidPayment();
        payment.PaymentId = Guid.Empty;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].PaymentId|PaymentId is mandatory.");
    }

    [Test]
    public void ThenRejectsMissingAccountId()
    {
        var payment = CreateValidPayment();
        payment.AccountId = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].AccountId|AccountId is mandatory and must be > 0.");
    }

    [Test]
    public void ThenRejectsMissingCollectionPeriodId()
    {
        var payment = CreateValidPayment();
        payment.CollectionPeriodId = " ";

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].CollectionPeriodId|CollectionPeriodId is mandatory.");
    }

    private static PaymentStagingModel CreateValidPayment()
    {
        return new PaymentStagingModel
        {
            PaymentId = Guid.NewGuid(),
            AccountId = 123,
            Ukprn = 10000001,
            Uln = 1234567890,
            ApprenticeshipId = 456,
            CollectionPeriodId = "2526-R03",
            DeliveryPeriodMonth = 10,
            DeliveryPeriodYear = 2025,
            CollectionPeriodMonth = 10,
            CollectionPeriodYear = 2025,
            Amount = 100m
        };
    }
}
