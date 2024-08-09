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
        logger.LogInformation("{HandlerName} started for AccountId: {AccountId} and PaymentId: {PaymentId} PeriodEndRef: {PeriodEndRef}.",
            nameof(RefreshPaymentMetadataCommandHandler),
            request.AccountId,
            request.PaymentId,
            request.PeriodEndRef);

        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            logger.LogWarning("{HandlerName}: request is not valid.", nameof(RefreshPaymentMetadataCommandHandler));
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        logger.LogInformation("{HandlerName}: request is valid.", nameof(RefreshPaymentMetadataCommandHandler));

        var payment = await levyRepository.GetPaymentForPaymentDetails(request.PaymentId);

        if (payment == null)
        {
            logger.LogWarning("{HandlerName}: No payment found with Id {PaymentId}.", nameof(RefreshPaymentMetadataCommandHandler), request.PaymentId);
        }
        else if (!string.IsNullOrEmpty(payment.ProviderName) && !string.IsNullOrEmpty(payment.ApprenticeName))
        {
            logger.LogInformation("{HandlerName}: Payment with Id {PaymentId} already has ProviderName and ApprenticeName populated.", 
                nameof(RefreshPaymentMetadataCommandHandler), request.PaymentId);
        }
        else
        {
            logger.LogInformation("{HandlerName}: Found payment from DB, executing AddSinglePaymentDetailsMetadata(): {Payment}.",
                nameof(RefreshPaymentMetadataCommandHandler), JsonSerializer.Serialize(payment));
            
            await paymentService.AddSinglePaymentDetailsMetadata(request.AccountId, payment);

            logger.LogInformation("{HandlerName}: Saving PaymentDetails: {PaymentDetails}.",
                nameof(RefreshPaymentMetadataCommandHandler), 
                JsonSerializer.Serialize(payment));

            await levyRepository.UpdatePaymentMetadata(payment);
        }

        logger.LogInformation("{HandlerName} completed.", nameof(RefreshPaymentMetadataCommandHandler));
    }
}