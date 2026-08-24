using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionHistory;

public class GetEnglishFractionHistoryQueryValidator : IValidator<GetEnglishFractionHistoryQuery>
{
    public ValidationResult Validate(GetEnglishFractionHistoryQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError(nameof(item.HashedAccountId));
        }

        if (string.IsNullOrWhiteSpace(item.EmpRef))
        {
            validationResult.AddError(nameof(item.EmpRef));
        }
        else if (!EmployerReferenceValidation.TryNormalise(item.EmpRef, out var normalisedEmployerReference))
        {
            validationResult.AddError(nameof(item.EmpRef), "EmpRef must be a valid PAYE reference");
        }
        else
        {
            item.EmpRef = normalisedEmployerReference;
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(
        GetEnglishFractionHistoryQuery item)
    {
        return Task.FromResult(Validate(item));
    }
}