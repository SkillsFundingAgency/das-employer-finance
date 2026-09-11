using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummaryByAccountId;

public class GetLevySummaryByAccountIdQueryValidator : IValidator<GetLevySummaryByAccountIdQuery>
{
    public ValidationResult Validate(GetLevySummaryByAccountIdQuery item)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(GetLevySummaryByAccountIdQuery item)
    {
        var result = new ValidationResult();

        if (item.AccountId <= 0)
        {
            result.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        return Task.FromResult(result);
    }
}