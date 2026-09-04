using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByHashedAccountId;

public class GetLevySummaryByHashedAccountIdQueryValidator : IValidator<GetLevySummaryByHashedAccountIdQuery>
{
    public ValidationResult Validate(GetLevySummaryByHashedAccountIdQuery item)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(GetLevySummaryByHashedAccountIdQuery item)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            result.AddError(nameof(item.HashedAccountId), "HashedAccountId has not been supplied");
        }

        return Task.FromResult(result);
    }
}