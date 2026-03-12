using FluentAssertions;
using Moq;
using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.BulkPaymentsIngestTests;

public class WhenIHandleTheCommand
{
    private BulkPaymentsIngestCommandHandler _handler;
    private Mock<IPaymentStagingRepository> _paymentStagingRepository;
    private Mock<IValidator<BulkPaymentsIngestCommand>> _validator;
    private Mock<ILogger<BulkPaymentsIngestCommandHandler>> _logger;

    [SetUp]
    public void Arrange()
    {
        _paymentStagingRepository = new Mock<IPaymentStagingRepository>();
        _validator = new Mock<IValidator<BulkPaymentsIngestCommand>>();
        _logger = new Mock<ILogger<BulkPaymentsIngestCommandHandler>>();

        _handler = new BulkPaymentsIngestCommandHandler(
            _validator.Object,
            _paymentStagingRepository.Object,
            _logger.Object);
    }

    [Test]
    public async Task ThenThePaymentsAreInsertedAndResponseReturned()
    {
        // Arrange
        var payments = new List<PaymentStagingModel>
        {
            new()
            {
                PaymentId = Guid.NewGuid(),
                AccountId = 123,
                Ukprn = 12345678,
                Uln = 1234567890,
                ApprenticeshipId = 456,
                CollectionPeriodId = "R01",
                DeliveryPeriodMonth = 1,
                DeliveryPeriodYear = 2025,
                CollectionPeriodMonth = 1,
                CollectionPeriodYear = 2025,
                Amount = 100
            }
        };

        var command = new BulkPaymentsIngestCommand
        {
            Payments = payments
        };

        var validationResult = new ValidationResult();

        _validator
            .Setup(v => v.Validate(It.IsAny<BulkPaymentsIngestCommand>()))
            .Returns(validationResult);

        _paymentStagingRepository
            .Setup(x => x.GetExistingPaymentIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Guid>());

        _paymentStagingRepository
          .Setup(x => x.BulkInsertPaymentsAsync(It.IsAny<List<PaymentStagingModel>>()))
          .ReturnsAsync(new BulkPaymentsIngestResult
          {
              IsSuccess = true,
              InsertedCount = 1,
              PaymentIds = payments.Select(p => p.PaymentId).ToList()
          });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.InsertedCount.Should().Be(1);
        result.PaymentIds.Should().BeEquivalentTo(payments.Select(p => p.PaymentId));

        _paymentStagingRepository.Verify(
            x => x.BulkInsertPaymentsAsync(
                It.Is<List<PaymentStagingModel>>(p => p.Count == 1)),
            Times.Once);
    }

    [Test]
    public async Task ThenReturnsFailureResponseWhenRepositoryThrows()
    {
        // Arrange
        var payments = new List<PaymentStagingModel>
        {
            new()
            {
                PaymentId = Guid.NewGuid(),
                AccountId = 123,
                Ukprn = 12345678,
                Uln = 1234567890,
                ApprenticeshipId = 456,
                CollectionPeriodId = "R01",
                DeliveryPeriodMonth = 1,
                DeliveryPeriodYear = 2025,
                CollectionPeriodMonth = 1,
                CollectionPeriodYear = 2025,
                Amount = 100
            }
        };

        var command = new BulkPaymentsIngestCommand
        {
            Payments = payments
        };

        var validationResult = new ValidationResult();

        _validator
            .Setup(v => v.Validate(It.IsAny<BulkPaymentsIngestCommand>()))
            .Returns(validationResult);

        _paymentStagingRepository
            .Setup(x => x.GetExistingPaymentIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Guid>());

        _paymentStagingRepository
            .Setup(x => x.BulkInsertPaymentsAsync(It.IsAny<List<PaymentStagingModel>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();

        _paymentStagingRepository.Verify(
            x => x.BulkInsertPaymentsAsync(It.IsAny<List<PaymentStagingModel>>()),
            Times.Once);
    }
}