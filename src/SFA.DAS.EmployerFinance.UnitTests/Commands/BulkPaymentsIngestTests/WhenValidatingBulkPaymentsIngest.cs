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
    public void ThenRejectsMissingApprenticeshipId()
    {
        var payment = CreateValidPayment();
        payment.ApprenticeshipId = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].ApprenticeshipId|ApprenticeshipId is mandatory and must be > 0.");
    }

    [Test]
    public void ThenRejectsMissingUln()
    {
        var payment = CreateValidPayment();
        payment.Uln = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].Uln|Uln is mandatory and must be > 0.");
    }

    [Test]
    public void ThenRejectsInvalidDeliveryPeriodMonth()
    {
        var payment = CreateValidPayment();
        payment.DeliveryPeriodMonth = 0;

        var result = _validator.Validate(new BulkPaymentsIngestCommand
        {
            Payments = [payment]
        });

        result.IsValid().Should().BeFalse();
        result.ErrorList.Should().Contain("Payments[0].DeliveryPeriodMonth|DeliveryPeriodMonth must be between 1 and 12.");
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
