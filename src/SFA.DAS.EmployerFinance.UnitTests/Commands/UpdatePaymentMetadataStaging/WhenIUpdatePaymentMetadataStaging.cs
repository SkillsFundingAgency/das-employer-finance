using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // default: valid request
            _validator
                .Setup(v => v.Validate(It.IsAny<UpdatePaymentMetadataStagingCommand>()))
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
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = paymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            _repository.Setup(r => r.PaymentStagingExists(paymentId))
                       .ReturnsAsync(true);
            _repository.Setup(r => r.UpdatePaymentMetadataStaging(paymentId, command.PaymentMetadataStaging))
                       .ReturnsAsync(123);

            var result = await _handler.Handle(command, CancellationToken.None);

            _validator.Verify(v => v.Validate(command), Times.Once);
            _repository.Verify(r => r.UpdatePaymentMetadataStaging(paymentId, command.PaymentMetadataStaging), Times.Once);

            Assert.That(result.Upserted, Is.True);
            Assert.That(result.MetadataId, Is.EqualTo(123));
        }

        [Test]
        public async Task ThenReturnsValidationErrors_WhenCommandIsInvalid()
        {
            // Arrange: make validator return errors
            _validator
                .Setup(v => v.Validate(It.IsAny<UpdatePaymentMetadataStagingCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string> { { "field", "error" } }
                });

            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = Guid.NewGuid(),
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result.HasValidationErrors, Is.True);
            Assert.That(result.ValidationErrors.Count, Is.EqualTo(1));
            Assert.That(result.ValidationErrors.First(), Is.EqualTo("error"));
        }

        [Test]
        public async Task ThenReturnsNotFound_WhenPaymentDoesNotExist()
        {
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = paymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            _repository.Setup(r => r.PaymentStagingExists(paymentId)).ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.NotFound, Is.True);
        }

        [Test]
        public async Task ThenReturnsSuccess_WhenMessageIsValidAndExists()
        {
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = paymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging()
            };

            _repository.Setup(r => r.PaymentStagingExists(paymentId)).ReturnsAsync(true);
            _repository.Setup(r => r.UpdatePaymentMetadataStaging(paymentId, command.PaymentMetadataStaging)).ReturnsAsync(123);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Upserted, Is.True);
            Assert.That(result.MetadataId, Is.EqualTo(123));
        }

        [Test]
        public async Task ThenPassesAppUnitMetadataFieldsToRepository()
        {
            var paymentId = Guid.NewGuid();
            var command = new UpdatePaymentMetadataStagingCommand
            {
                PaymentId = paymentId,
                PaymentMetadataStaging = new PaymentMetaDataStaging
                {
                    LearningType = "ApprenticeshipUnit",
                    CourseCode = "ST0001",
                    CohortId = 123456
                }
            };

            _repository.Setup(r => r.PaymentStagingExists(paymentId)).ReturnsAsync(true);
            _repository
                .Setup(r => r.UpdatePaymentMetadataStaging(
                    paymentId,
                    It.Is<PaymentMetaDataStaging>(metadata =>
                        metadata.LearningType == "ApprenticeshipUnit"
                        && metadata.CourseCode == "ST0001"
                        && metadata.CohortId == 123456)))
                .ReturnsAsync(123);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.IsSuccess, Is.True);
            _repository.Verify(r => r.UpdatePaymentMetadataStaging(
                paymentId,
                It.Is<PaymentMetaDataStaging>(metadata =>
                    metadata.LearningType == "ApprenticeshipUnit"
                    && metadata.CourseCode == "ST0001"
                    && metadata.CohortId == 123456)),
                Times.Once);
        }
    }
}
