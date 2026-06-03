using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.FindEmployerAccountExpiredFunds;

public class FindEmployerAccountExpiredFundsQueryValidator: IValidator<FindEmployerAccountExpiredFundsQuery>
{
    public ValidationResult Validate(FindEmployerAccountExpiredFundsQuery item)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(FindEmployerAccountExpiredFundsQuery item)
    {
        var result = new ValidationResult();

        if (item.FromDate == DateTime.MinValue)
        {
            result.AddError(nameof(item.FromDate), "From date has not been supplied");
        }
        if (item.ToDate == DateTime.MinValue)
        {
            result.AddError(nameof(item.ToDate), "To date has not been supplied");
        }
        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            result.AddError(nameof(item.HashedAccountId), "HashedAccountId has not been supplied");
        }

        return result;
    }
}