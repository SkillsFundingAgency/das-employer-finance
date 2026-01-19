using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Validation;
using System.Text.RegularExpressions;

public class UpdatePaymentMetadataStagingCommandValidatorStaging
    : IValidator<UpdatePaymentMetadataStagingCommand>
{
    public ValidationResult Validate(UpdatePaymentMetadataStagingCommand item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrWhiteSpace(item.PaymentMetadataStaging?.ProviderName))
            validationResult.AddError("Provider", "Provider is required");

        if (!IsValidNiNumber(item.PaymentMetadataStaging.ApprenticeNINumber))
            validationResult.AddError("NationalInsuranceNumber", "Invalid NI format");

        if (item.PaymentMetadataStaging?.ApprenticeshipCourseStartDate < new DateTime(1900, 1, 1))
            validationResult.AddError("StartDate", "StartDate must be after 1900-01-01");

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(UpdatePaymentMetadataStagingCommand item)
        => Task.FromResult(Validate(item));

    private static readonly Regex NiRegex = new(
      @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)[A-CEGHJ-PR-TW-Z]{2}\d{6}[A-D]$",
      RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private bool IsValidNiNumber(string ni)
    {
        if (string.IsNullOrWhiteSpace(ni))
            return false;

        var normalized = ni.Replace(" ", "").ToUpperInvariant();

        return NiRegex.IsMatch(normalized);
    }
}