using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommandHandler(
    IValidator<RefreshPaymentMetadataCommand> validator,
    IPaymentService paymentService,
    Lazy<EmployerFinanceDbContext> financeDbContext,
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

        var currentPayment = await financeDbContext.Value.Payments
            .Where(p => p.Id == request.PaymentId)
            .Select(p => new PaymentDetails
            {
                Id = p.Id,
                ApprenticeshipId = p.ApprenticeshipId,
                Ukprn = p.Ukprn,
                PaymentMetaDataId = p.PaymentMetaDataId,
                StandardCode = p.StandardCode,
                FrameworkCode = p.FrameworkCode,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentPayment == null)
        {
            logger.LogWarning("{HandlerName}: No payment found with Id {PaymentId}.", nameof(RefreshPaymentMetadataCommandHandler), request.PaymentId);
        }
        else
        {
            logger.LogInformation("{HandlerName}: Found payment {PaymentId} with ApprenticeshipId = {ApprenticeshipId}. Executing AddSinglePaymentDetailsMetadata().", nameof(RefreshPaymentMetadataCommandHandler), currentPayment.Id, currentPayment.ApprenticeshipId);

            await paymentService.AddSinglePaymentDetailsMetadata(request.AccountId, currentPayment);

            logger.LogInformation("{HandlerName}: Saving PaymentDetails: {PaymentDetails}.", nameof(RefreshPaymentMetadataCommandHandler), JsonSerializer.Serialize(currentPayment));

            await levyRepository.UpdatePaymentMetadata(currentPayment);
        }

        logger.LogInformation("{HandlerName} completed.", nameof(RefreshPaymentMetadataCommandHandler));
    }
}