using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadata;
using SFA.DAS.EmployerFinance.Validation;
using StructureMap.Diagnostics;
using System.Text.RegularExpressions;

public class UpdatePaymentMetadataCommandValidator
    : IValidator<UpdatePaymentMetadataCommand>
{
    public ValidationResult Validate(UpdatePaymentMetadataCommand item)
    {
        var validationResult = new ValidationResult();

        if (item.PaymentMetadata.Id<= 0)
            validationResult.AddError(nameof(item.PaymentMetadata.Id), "PaymentId is required");

        if (string.IsNullOrWhiteSpace(item.PaymentMetadata?.ProviderName))
            validationResult.AddError("Provider", "Provider is required");

        if (!IsValidNiNumber(item.PaymentMetadata.ApprenticeNINumber))
            validationResult.AddError("NationalInsuranceNumber", "Invalid NI format");

        if (item.PaymentMetadata?.ApprenticeshipCourseStartDate < new DateTime(1900, 1, 1))
            validationResult.AddError("StartDate", "StartDate must be after 1900-01-01");

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(UpdatePaymentMetadataCommand item)
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