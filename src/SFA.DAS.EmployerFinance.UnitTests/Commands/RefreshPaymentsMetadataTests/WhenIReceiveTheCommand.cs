using System.ComponentModel.DataAnnotations;
using AutoFixture.NUnit3;
using SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.EmployerFinance.Validation.ValidationResult;

namespace SFA.DAS.EmployerFinance.UnitTests.Commands.RefreshPaymentsMetadataTests;

public class WhenIReceiveTheCommand
{
    [Test, MoqAutoData]
    public async Task ThenTheCommandIsValidated(
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        PaymentDetails payment)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult())
            .Verifiable(Times.Once);

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(It.Is<Guid>(p => p.Equals(command.PaymentId))))
            .ReturnsAsync(payment);

        await handler.Handle(command, CancellationToken.None);

        validator.Verify();
    }

    [Test, MoqAutoData]
    public void ThenAnInvalidRequestExceptionIsThrownIfTheCommandIsNotValid(
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [NoAutoProperties] RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler
    )
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult { ValidationDictionary = new Dictionary<string, string> { { "", "" } } });

        var result = async () => await handler.Handle(command, CancellationToken.None);

        result.ShouldThrow<ValidationException>();
    }

    [Test, MoqAutoData]
    public async Task ThenTheLevyRepositoryIsCalledToGetPayment(
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler,
        PaymentDetails payment)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(payment)
            .Verifiable(Times.Once);

        await handler.Handle(command, CancellationToken.None);

        levyRepository.Verify();
    }

    [Test, MoqAutoData]
    public async Task ThenAddSinglePaymentDetailsMetadataIsNotCalledWhenPaymentIsNull(
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(() => null);

        await handler.Handle(command, CancellationToken.None);

        paymentService.Verify(x => x.AddSinglePaymentDetailsMetadata(It.IsAny<long>(), It.IsAny<PaymentDetails>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task ThenAddSinglePaymentDetailsMetadataIsNotCalledWhenPaymentAlreadyHasMetadata(
        PaymentDetails payment,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(() => payment);

        await handler.Handle(command, CancellationToken.None);

        paymentService.Verify(x => x.AddSinglePaymentDetailsMetadata(It.IsAny<long>(), It.IsAny<PaymentDetails>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task ThenUpdatePaymentMetadataIsNotCalledWhenPaymentIsNull(
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(() => null);

        await handler.Handle(command, CancellationToken.None);

        levyRepository.Verify(x => x.UpdatePaymentMetadata(It.IsAny<PaymentDetails>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task ThenUpdatePaymentMetadataIsNotCalledWhenPaymentPaymentAlreadyHasMetadata(
        PaymentDetails payment,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        RefreshPaymentMetadataCommand command,
        RefreshPaymentMetadataCommandHandler handler)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(() => payment)
            .Verifiable();

        await handler.Handle(command, CancellationToken.None);

        levyRepository.Verify(x => x.UpdatePaymentMetadata(It.IsAny<PaymentDetails>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task ThenUpdatePaymentMetadataIsCalledWhenPaymentIsNotNull(
        RefreshPaymentMetadataCommand command,
        [Frozen] PaymentDetails paymentDetails,
        PaymentDetails paymentFromDb,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        RefreshPaymentMetadataCommandHandler handler
    )
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());

        command.PaymentId = paymentFromDb.Id;
        paymentFromDb.ApprenticeName = string.Empty;
        paymentFromDb.ProviderName = string.Empty;

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(paymentFromDb);

        paymentService.Setup(x => x.AddSinglePaymentDetailsMetadata(command.AccountId, paymentFromDb)).ReturnsAsync(paymentDetails);

        await handler.Handle(command, CancellationToken.None);
        
        levyRepository.Verify(x => x.UpdatePaymentMetadata(paymentDetails), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task ThenAddSinglePaymentDetailsMetadataIsCalledWhenPaymentIsNotNull(
        RefreshPaymentMetadataCommand command,
        [Frozen] PaymentDetails paymentDetails,
        PaymentDetails paymentFromDb,
        [Frozen] Mock<IValidator<RefreshPaymentMetadataCommand>> validator,
        [Frozen] Mock<IPaymentService> paymentService,
        [Frozen] Mock<IDasLevyRepository> levyRepository,
        RefreshPaymentMetadataCommandHandler handler)
    {
        validator.Setup(x =>
                x.Validate(It.Is<RefreshPaymentMetadataCommand>(c =>
                    c.PaymentId == command.PaymentId
                    && c.AccountId == command.AccountId
                    && c.PeriodEndRef == command.PeriodEndRef)))
            .Returns(new ValidationResult());
        
        command.PaymentId = paymentFromDb.Id;
        paymentFromDb.ApprenticeName = string.Empty;
        paymentFromDb.ProviderName = string.Empty;

        levyRepository.Setup(x =>
                x.GetPaymentForPaymentDetails(command.PaymentId))
            .ReturnsAsync(paymentFromDb);
        
        paymentService.Setup(x => x.AddSinglePaymentDetailsMetadata(command.AccountId, paymentFromDb)).ReturnsAsync(paymentDetails);

        await handler.Handle(command, CancellationToken.None);

        paymentService.Verify(x => x.AddSinglePaymentDetailsMetadata(command.AccountId, paymentFromDb), Times.Once);
    }
}