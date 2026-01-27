using Moq;
using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
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
        _logger = new Mock<ILogger<BulkPaymentsIngestCommandHandler>>();

        _handler = new BulkPaymentsIngestCommandHandler(
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

        var repositoryResult = new BulkPaymentsIngestResult
        {
            IsSuccess = true,
            InsertedCount = 1,
            PaymentIds = payments.Select(p => p.PaymentId).ToList(),
            Message = "Success"
        };

        _paymentStagingRepository
            .Setup(x => x.BulkInsertPaymentsAsync(It.IsAny<List<PaymentStagingModel>>()))
            .Returns(Task.FromResult(repositoryResult));

        var command = new BulkPaymentsIngestCommand();
        command.Payments = payments;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.InsertedCount.Should().Be(1);
        result.PaymentIds.Should().BeEquivalentTo(repositoryResult.PaymentIds);
        result.Message.Should().Be("Success");

        _paymentStagingRepository.Verify(
            x => x.BulkInsertPaymentsAsync(
                It.Is<List<PaymentStagingModel>>(p => p.Count == 1)),
            Times.Once);
    }


    [Test]
    public async Task ThenAnExceptionIsThrownWhenRepositoryFails()
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

        _paymentStagingRepository
            .Setup(x => x.BulkInsertPaymentsAsync(It.IsAny<List<PaymentStagingModel>>()))
            .Returns(Task.FromException<BulkPaymentsIngestResult>(
                new Exception("Database error")));

        var command = new BulkPaymentsIngestCommand();
        command.Payments = payments;

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        _paymentStagingRepository.Verify(
            x => x.BulkInsertPaymentsAsync(
                It.Is<List<PaymentStagingModel>>(p => p.Count == 1)),
            Times.Once);
    }

}
