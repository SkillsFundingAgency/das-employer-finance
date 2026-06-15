using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetPayeSchemesByEmployerId;

public class GetPayeSchemesByEmployerIdValidator : IValidator<GetPayeSchemesByEmployerIdQuery>
{
    public ValidationResult Validate(GetPayeSchemesByEmployerIdQuery item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetPayeSchemesByEmployerIdQuery item)
    {
        throw new NotImplementedException();
    }
}
