using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountProviderPayments;

public class FindAccountProviderPaymentsQueryValidator : IValidator<FindAccountProviderPaymentsQuery>
{
    public ValidationResult Validate(FindAccountProviderPaymentsQuery item)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(FindAccountProviderPaymentsQuery item)
    {
        var result = new ValidationResult();

        if (item.UkPrn == default(long))
        {
            result.AddError(nameof(item.UkPrn), "UkPrn has not been supplied");
        }

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