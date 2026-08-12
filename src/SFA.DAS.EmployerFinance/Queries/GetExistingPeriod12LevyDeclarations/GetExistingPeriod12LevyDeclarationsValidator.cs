using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingPeriod12LevyDeclarations;

public class GetExistingPeriod12LevyDeclarationsValidator : IValidator<GetExistingPeriod12LevyDeclarationsQuery>
{
    public ValidationResult Validate(GetExistingPeriod12LevyDeclarationsQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.EmpRef))
        {
            validationResult.AddError(nameof(item.EmpRef));
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetExistingPeriod12LevyDeclarationsQuery item)
    {
        throw new NotImplementedException();
    }
}
