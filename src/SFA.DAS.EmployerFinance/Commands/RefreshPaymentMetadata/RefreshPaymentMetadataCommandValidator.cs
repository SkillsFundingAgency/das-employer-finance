using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentMetadata;

public class RefreshPaymentMetadataCommandValidator : IValidator<RefreshPaymentMetadataCommand>
{
    public ValidationResult Validate(RefreshPaymentMetadataCommand item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId == 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        if (item.PaymentId == Guid.Empty)
        {
            validationResult.AddError(nameof(item.PaymentId), "PaymentId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(RefreshPaymentMetadataCommand item)
    {
        throw new System.NotImplementedException();
    }
}