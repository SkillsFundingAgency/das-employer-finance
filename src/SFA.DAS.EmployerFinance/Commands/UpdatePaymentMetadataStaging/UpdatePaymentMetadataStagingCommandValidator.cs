using SFA.DAS.EmployerFinance.Commands.UpdatePaymentMetadataStaging;
using SFA.DAS.EmployerFinance.Validation;
using System.Text.RegularExpressions;

public class UpdatePaymentMetadataStagingCommandValidator
    : IValidator<UpdatePaymentMetadataStagingCommand>
{
    public ValidationResult Validate(UpdatePaymentMetadataStagingCommand item)
    {
        var validationResult = new ValidationResult();

        if (item.PaymentId == Guid.Empty)
            validationResult.AddError("PaymentId", "PaymentId is required");

        if (item.PaymentMetadataStaging == null)
        {
            validationResult.AddError("PaymentMetadataStaging", "PaymentMetadataStaging is required");
            return validationResult;
        }

        if (!string.IsNullOrWhiteSpace(item.PaymentMetadataStaging.ApprenticeNINumber) &&
            !IsValidNiNumber(item.PaymentMetadataStaging.ApprenticeNINumber))
        {
            validationResult.AddError("ApprenticeNINumber", "Invalid NI format");
        }

        var startDate = item.PaymentMetadataStaging.ApprenticeshipCourseStartDate;

        if (startDate.HasValue && startDate <= new DateTime(1900, 1, 1))
        {
            validationResult.AddError("ApprenticeshipCourseStartDate", "StartDate must be after 1900-01-01");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(UpdatePaymentMetadataStagingCommand item)
        => Task.FromResult(Validate(item));

    private static readonly Regex NiRegex = new(
        @"^(?!BG)(?!GB)(?!NK)(?!KN)(?!TN)(?!NT)(?!ZZ)[A-CEGHJ-PR-TW-Z]{2}\d{6}[A-D]$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private bool IsValidNiNumber(string ni)
    {
        var normalized = ni.Replace(" ", "").ToUpperInvariant();

        return NiRegex.IsMatch(normalized);
    }
}
