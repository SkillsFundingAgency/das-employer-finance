using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetEmployerAccount;

public class GetEmployerAccountByHashedIdValidator : IValidator<GetEmployerAccountHashedQuery>
{

    public ValidationResult Validate(GetEmployerAccountHashedQuery item)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(GetEmployerAccountHashedQuery item)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(item.UserId))
        {
            result.AddError(nameof(item.UserId), "UserId has not been supplied");
        }
        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            result.AddError(nameof(item.HashedAccountId), "HashedAccountId has not been supplied");
        }

        
        return result;
    }
}