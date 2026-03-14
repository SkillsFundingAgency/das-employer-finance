using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetGovernmentGatewayOnlySchemesByEmployerId;

public class GetGovernmentGatewayOnlySchemesByEmployerIdValidator : IValidator<GetGovernmentGatewayOnlySchemesByEmployerIdQuery>
{
    public ValidationResult Validate(GetGovernmentGatewayOnlySchemesByEmployerIdQuery item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetGovernmentGatewayOnlySchemesByEmployerIdQuery item)
    {
        throw new NotImplementedException();
    }
}
