using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommandHandler(
    IValidator<RefreshPaymentMetadataCommand> validator,
    IPaymentService paymentService,
    ILogger<RefreshPaymentMetadataCommandHandler> logger,
    IDasLevyRepository levyRepository)
    : IRequestHandler<RefreshPaymentMetadataCommand>
{
    public async Task Handle(RefreshPaymentMetadataCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("{HandlerName} started.", nameof(RefreshPaymentMetadataCommandHandler));

        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            var exception = new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
            logger.LogError(exception, "{HandlerName}: request is not valid. Request: {Request}", nameof(RefreshPaymentMetadataCommandHandler), JsonSerializer.Serialize(request));
            throw exception;
        }

        logger.LogInformation("{HandlerName}: request is valid.", nameof(RefreshPaymentMetadataCommandHandler));
        
        var payment = await levyRepository.GetPaymentForPaymentDetails(request.PaymentId, cancellationToken);

        if (payment == null)
        {
            logger.LogWarning("{HandlerName}: No payment found with Id {PaymentId}.", nameof(RefreshPaymentMetadataCommandHandler), request.PaymentId);
        }
        else
        {
            logger.LogInformation("{HandlerName}: Payment from DB: {Payment}.", nameof(RefreshPaymentMetadataCommandHandler), JsonSerializer.Serialize(payment));
            
            var currentPayment = new PaymentDetails
            {
                Id = payment.Id,
                ApprenticeshipId = payment.ApprenticeshipId,
                Ukprn = payment.Ukprn,
                PaymentMetaDataId = payment.PaymentMetaDataId,
                StandardCode = payment.StandardCode,
                FrameworkCode = payment.FrameworkCode,
                PathwayCode = payment.PathwayCode,
                ProgrammeType = payment.ProgrammeType
            };
            
            logger.LogInformation("{HandlerName}: Found payment {PaymentId} with ApprenticeshipId = {ApprenticeshipId}. Executing AddSinglePaymentDetailsMetadata().", nameof(RefreshPaymentMetadataCommandHandler), currentPayment.Id, currentPayment.ApprenticeshipId);

            await paymentService.AddSinglePaymentDetailsMetadata(request.AccountId, currentPayment);

            logger.LogInformation("{HandlerName}: Saving PaymentDetails: {PaymentDetails}.", nameof(RefreshPaymentMetadataCommandHandler), JsonSerializer.Serialize(currentPayment));

            await levyRepository.UpdatePaymentMetadata(currentPayment);
        }

        logger.LogInformation("{HandlerName} completed.", nameof(RefreshPaymentMetadataCommandHandler));
    }
}