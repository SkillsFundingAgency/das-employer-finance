using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevySummary;

public class GetLevySummaryQueryValidator : IValidator<GetLevySummaryQuery>
{
    public ValidationResult Validate(GetLevySummaryQuery item)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(GetLevySummaryQuery item)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            result.AddError(nameof(item.HashedAccountId), "HashedAccountId has not been supplied");
        }

        return result;
    }
}