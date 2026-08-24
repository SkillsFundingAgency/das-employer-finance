using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;

public class GetLastEnglishFractionCalculationDateValidator : IValidator<GetLastEnglishFractionCalculationDateQuery>
{
    public ValidationResult Validate(GetLastEnglishFractionCalculationDateQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.EmpRef))
        {
            validationResult.AddError(nameof(item.EmpRef));
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetLastEnglishFractionCalculationDateQuery item)
    {
        throw new NotImplementedException();
    }
}
