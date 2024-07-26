using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Services.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommandHandler(
    IValidator<RefreshPaymentMetadataCommand> validator,
    IPaymentService paymentService,
    Lazy<EmployerFinanceDbContext> financeDbContext,
    ILogger<RefreshPaymentMetadataCommandHandler> logger)
    : IRequestHandler<RefreshPaymentMetadataCommand, Unit>
{
    public async Task<Unit> Handle(RefreshPaymentMetadataCommand request, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        ICollection<PaymentDetails> payments = null;

        var currentPayment = await financeDbContext.Value.Payments
            .Where(p => p.Id == request.PaymentId)
            .Select(p => new PaymentDetails
            {
                Id = p.Id,
                ApprenticeshipId = p.ApprenticeshipId,
                Ukprn = p.Ukprn,
                PaymentMetaDataId = p.PaymentMetaDataId
            })
            .FirstOrDefaultAsync();
        
        logger.LogInformation("Found payment {PaymentId} with ApprenticeshipId = {ApprenticeshipId}", currentPayment.Id, currentPayment.ApprenticeshipId);
        
        await paymentService.AddSinglePaymentDetailsMetadata(request.PeriodEndRef, request.AccountId, currentPayment).ConfigureAwait(false);
        
        var paymentMetaData = await financeDbContext.Value.PaymentMetaData
            .Where(pmd => pmd.Id == currentPayment.PaymentMetaDataId)
            .SingleOrDefaultAsync()
            .ConfigureAwait(false);
        
        return Unit.Value;
    }
}