using global::SFA.DAS.EmployerFinance.Data.Contracts;
using global::SFA.DAS.EmployerFinance.Models.Payments;
using global::SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using System.ComponentModel.DataAnnotations;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.UpdatePaymentMetadataTests
{
    public class WhenIUpdatePaymentMetadataStaging
    {
        private UpdatePaymentMetadataStagingCommandHandler _handler;
        private Mock<IValidator<UpdatePaymentMetadataStagingCommand>> _validator;
        private Mock<IDasLevyRepository> _repository;

        [SetUp]
        public void Arrange()
        {
            _repository = new Mock<IDasLevyRepository>();

            _validator = new Mock<IValidator<UpdatePaymentMetadataStagingCommand>>();
            _validator
                .Setup(x => x.Validate(It.IsAny<UpdatePaymentMetadataStagingCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>()
                });

            _handler = new UpdatePaymentMetadataStagingCommandHandler(
                _validator.Object,
                _repository.Object);
        }

        [Test]
        public async Task ThenTheCommandIsValidated()
        {
            // Arrange
            var testPaymentId = Guid.NewGuid();

            _repository.Setup(x => x.UpdatePaymentMetadataStaging(
                testPaymentId,
                It.IsAny<PaymentMetaDataStaging>()))
                .ReturnsAsync(123);

            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = testPaymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _validator.Verify(
                x => x.Validate(It.IsAny<UpdatePaymentMetadataStagingCommand>()),
                Times.Once);

            _repository.Verify(x =>
                x.UpdatePaymentMetadataStaging(
                    testPaymentId,
                    command.PaymentMetadataStaging),
                Times.Once);
        }

        [Test]
        public void ThenAnInvalidRequestionExceptionIsThrownWhenTheMessageIsNotValid()
        {
            // Arrange
            _validator
                .Setup(x => x.Validate(It.IsAny<UpdatePaymentMetadataStagingCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string> { { "", "" } }
                });

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new UpdatePaymentMetadataStagingCommand(), CancellationToken.None));
        }

        [Test]
        public async Task ThenTheRepositoryIsCalledWhenTheMessageIsValid()
        {
            // Arrange
            var testPaymentId = Guid.NewGuid();
            var testMetadataId = 123;

            _repository.Setup(x => x.UpdatePaymentMetadataStaging(
                testPaymentId,
                It.IsAny<PaymentMetaDataStaging>()))
                .ReturnsAsync(testMetadataId);

            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = testPaymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repository.Verify(x =>
                x.UpdatePaymentMetadataStaging(
                    testPaymentId,
                    command.PaymentMetadataStaging),
                Times.Once);

            Assert.That(result.MetadataId, Is.EqualTo(testMetadataId));
            Assert.That(result.Upserted, Is.True);
        }
    }
}
