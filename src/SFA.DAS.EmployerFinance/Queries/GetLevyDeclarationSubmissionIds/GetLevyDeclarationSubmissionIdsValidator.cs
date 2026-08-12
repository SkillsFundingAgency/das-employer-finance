using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationSubmissionIds;

public class GetLevyDeclarationSubmissionIdsValidator : IValidator<GetLevyDeclarationSubmissionIdsQuery>
{
    public ValidationResult Validate(GetLevyDeclarationSubmissionIdsQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.EmpRef))
        {
            validationResult.AddError(nameof(item.EmpRef));
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetLevyDeclarationSubmissionIdsQuery item)
    {
        throw new NotImplementedException();
    }
}
