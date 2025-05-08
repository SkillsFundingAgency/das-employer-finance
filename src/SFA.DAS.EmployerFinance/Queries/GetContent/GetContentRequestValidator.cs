using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetContent;

public class GetContentRequestValidator : IValidator<GetContentQuery>
{
    public ValidationResult Validate(GetContentQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.ContentType))
        {
            validationResult.AddError(nameof(item.ContentType), "Type has not been supplied");
        }
        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(GetContentQuery item)
    {
        return Task.FromResult(Validate(item));
    }
}