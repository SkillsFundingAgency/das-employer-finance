using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetEnglishFractionCurrent;

public class GetEnglishFractionCurrentQueryValidator : IValidator<GetEnglishFractionCurrentQuery>
{
    public ValidationResult Validate(GetEnglishFractionCurrentQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError(nameof(item.HashedAccountId));
        }

        if (item.EmpRefs == null || item.EmpRefs.Length == 0)
        {
            validationResult.AddError(nameof(item.EmpRefs));
        }
        else
        {
            for (var index = 0; index < item.EmpRefs.Length; index++)
            {
                if (!EmployerReferenceValidation.TryNormalise(item.EmpRefs[index], out var normalisedEmployerReference))
                {
                    validationResult.AddError(nameof(item.EmpRefs), $"EmpRefs[{index}] must be a valid PAYE reference");
                    break;
                }

                item.EmpRefs[index] = normalisedEmployerReference;
            }
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetEnglishFractionCurrentQuery item)
    {
        return Task.FromResult(Validate(item));
    }
}