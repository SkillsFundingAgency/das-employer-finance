using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;
using SFA.DAS.EmployerFinance.Validation;
using StructureMap.Diagnostics;

public class UpdatePaymentMetadataCommandValidator
    : IValidator<UpdatePaymentMetadataCommand>
{
    public ValidationResult Validate(UpdatePaymentMetadataCommand item)
    {
        var errors = new List<ValidationError>();

        if (item.PaymentMetadata.Id<= 0)
            errors.Add(new ValidationError(nameof(item.PaymentId), "PaymentId is required"));

        if (string.IsNullOrWhiteSpace(item.PaymentMetadata?.Provider))
            errors.Add(new ValidationError("Provider", "Provider is required"));

        if (!IsValidNiNumber(item.PaymentMetadata?.NationalInsuranceNumber))
            errors.Add(new ValidationError("NationalInsuranceNumber", "Invalid NI format"));

        if (item.PaymentMetadata?.StartDate < new DateTime(1900, 1, 1))
            errors.Add(new ValidationError("StartDate", "StartDate must be after 1900-01-01"));

        return new ValidationResult(errors);
    }

    public Task<ValidationResult> ValidateAsync(UpdatePaymentMetadataCommand item)
        => Task.FromResult(Validate(item));

    private bool IsValidNiNumber(string ni)
    {
        // simple example – replace with real regex
        return !string.IsNullOrWhiteSpace(ni) && ni.Length >= 8;
    }
}