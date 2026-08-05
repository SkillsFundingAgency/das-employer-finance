using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.TransferStagedToOperationalTests;

public class WhenHandlingTransferStagedToOperational
{
    private Mock<IValidator<TransferStagedToOperationalCommand>> _validator;
    private Mock<IDasLevyRepository> _repository;
    private TransferStagedToOperationalCommandHandler _handler;

    [SetUp]
    public void Arrange()
    {
        _validator = new Mock<IValidator<TransferStagedToOperationalCommand>>();
        _repository = new Mock<IDasLevyRepository>();
        var logger = new Mock<ILogger<TransferStagedToOperationalCommandHandler>>();

        _validator
            .Setup(x => x.Validate(It.IsAny<TransferStagedToOperationalCommand>()))
            .Returns(new ValidationResult
            {
                ValidationDictionary = new Dictionary<string, string>()
            });

        _handler = new TransferStagedToOperationalCommandHandler(
            _validator.Object,
            _repository.Object,
            logger.Object);
    }

    [Test]
    public async Task Then_Returns_Validation_Errors_When_Command_Is_Invalid()
    {
        _validator
            .Setup(x => x.Validate(It.IsAny<TransferStagedToOperationalCommand>()))
            .Returns(new ValidationResult
            {
                ValidationDictionary = new Dictionary<string, string>
                {
                    { "AccountId", "AccountId must be greater than 0" }
                }
            });

        var result = await _handler.Handle(new TransferStagedToOperationalCommand(), CancellationToken.None);

        result.HasValidationErrors.Should().BeTrue();
        result.ValidationErrors.Should().Contain("AccountId must be greater than 0");
        _repository.Verify(
            x => x.TransferStagedToOperational(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Then_Calls_Repository_And_Returns_ProcessedCount()
    {
        _repository
            .Setup(x => x.TransferStagedToOperational(123, "2526-R01"))
            .ReturnsAsync(5);

        var result = await _handler.Handle(new TransferStagedToOperationalCommand
        {
            AccountId = 123,
            PeriodEnd = "2526-R01",
            CorrelationId = "corr-1"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(5);
        result.Message.Should().Be("Successfully transferred 5 staged rows to operational.");
        _repository.Verify(x => x.TransferStagedToOperational(123, "2526-R01"), Times.Once);
    }

    [Test]
    public async Task Then_Returns_Failure_When_Repository_Throws()
    {
        _repository
            .Setup(x => x.TransferStagedToOperational(123, "2526-R01"))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await _handler.Handle(new TransferStagedToOperationalCommand
        {
            AccountId = 123,
            PeriodEnd = "2526-R01",
            CorrelationId = "corr-1"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("An unexpected error occurred while transferring staged data to operational.");
    }
}
