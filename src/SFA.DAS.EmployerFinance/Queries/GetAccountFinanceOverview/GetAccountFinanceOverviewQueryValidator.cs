using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetAccountFinanceOverview;

public class GetAccountFinanceOverviewQueryValidator : IValidator<GetAccountFinanceOverviewQuery>
{
    public ValidationResult Validate(GetAccountFinanceOverviewQuery query)
    {
        throw new NotImplementedException();
    }

    public async Task<ValidationResult> ValidateAsync(GetAccountFinanceOverviewQuery query)
    {
        var result = new ValidationResult();

        result.IsUnauthorized = true;

        return result;
    }
}