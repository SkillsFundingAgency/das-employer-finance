using global::SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;
using global::SFA.DAS.EmployerFinance.Data.Contracts;
using global::SFA.DAS.EmployerFinance.Models.Payments;
using global::SFA.DAS.EmployerFinance.Validation;
using System.ComponentModel.DataAnnotations;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdatePaymentMetadataTests
{
    public class WhenIUpdatePaymentMetadata
    {
        private UpdatePaymentMetadataCommandHandler _handler;
        private Mock<IValidator<UpdatePaymentMetadataCommand>> _validator;
        private Mock<IDasLevyRepository> _repository;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<IDasLevyRepository>();

            _validator = new Mock<IValidator<UpdatePaymentMetadataCommand>>();
            _validator
                .Setup(x => x.Validate(It.IsAny<UpdatePaymentMetadataCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>()
                });

            _handler = new UpdatePaymentMetadataCommandHandler(
                _validator.Object,
                _repository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsValidated()
        {
            // Act
            await _handler.Handle(new UpdatePaymentMetadataCommand(), CancellationToken.None);

            // Assert
            _validator.Verify(
                x => x.Validate(It.IsAny<UpdatePaymentMetadataCommand>()),
                Times.Once);
        }

        [Test]
        public void ThenAnInvalidRequestionExceptionIsThrownWhenTheMessageIsNotValid()
        {
            // Arrange
            _validator
                .Setup(x => x.Validate(It.IsAny<UpdatePaymentMetadataCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string> { { "", "" } }
                });

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new UpdatePaymentMetadataCommand(), CancellationToken.None));
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledWhenTheMessageIsValid()
        {
            // Arrange
            var command = new UpdatePaymentMetadataCommand
            {
                PaymentId = Guid.NewGuid(),
                PaymentMetadata = new PaymentMetaData()
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repository.Verify(x =>
                x.UpdatePaymentMetadata(
                    command.PaymentId,
                    command.PaymentMetadata),
                Times.Once);
        }
    }
}
