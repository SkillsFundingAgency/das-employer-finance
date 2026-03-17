using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;

public class PersistEnglishFractionsCommandValidator : IValidator<PersistEnglishFractionsCommand>
{
    public ValidationResult Validate(PersistEnglishFractionsCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(item.EmployerReference))
        {
            validationResult.AddError(nameof(item.EmployerReference), "EmployerReference has not been supplied");
        }

        if (item.DateCalculated == DateTime.MinValue)
        {
            validationResult.AddError(nameof(item.DateCalculated), "DateCalculated has not been supplied");
        }

        if (item.Fractions == null || item.Fractions.Count == 0)
        {
            validationResult.AddError(nameof(item.Fractions), "Fractions payload is required.");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(PersistEnglishFractionsCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}
