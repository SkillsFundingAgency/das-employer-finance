using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetLevyDeclarationsByAccountAndDateRange;

public sealed record GetLevyDeclarationsByAccountAndDateRangeValidator : IValidator<GetLevyDeclarationsByAccountAndDateRangeQuery>
{
    public ValidationResult Validate(GetLevyDeclarationsByAccountAndDateRangeQuery item)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(GetLevyDeclarationsByAccountAndDateRangeQuery item)
    {
        try
        {
            var result = new ValidationResult();

            if (item.AccountId == 0)
            {
                result.AddError(nameof(item.AccountId), "AccountId has not been supplied");
            }

            if (item.FromDate == default)
            {
                result.AddError(nameof(item.FromDate), "FromDate has not been supplied");
            }

            if (item.ToDate == default) {
                result.AddError(nameof(item.ToDate), "ToDate has not been supplied");
            }

            if(item.ToDate < item.FromDate)
            {
                result.AddError(nameof(item.ToDate), "ToDate cannot be earlier than FromDate");
            }

            return Task.FromResult(result);
        }
        catch (Exception exception)
        {
            return Task.FromException<ValidationResult>(exception);
        }
    }
}