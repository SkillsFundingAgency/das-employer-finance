using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;

public class FindAccountCoursePaymentsQueryValidator : IValidator<FindAccountCoursePaymentsQuery>
{
    public ValidationResult Validate(FindAccountCoursePaymentsQuery item)
    {
        throw new NotImplementedException();
    }

    //TODO this is not tested
    public Task<ValidationResult> ValidateAsync(FindAccountCoursePaymentsQuery item)
    {
        var validationResult = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            validationResult.AddError(nameof(item.HashedAccountId), "Hashed Account ID has not been supplied");
        }

        if (item.UkPrn == 0)
        {
            validationResult.AddError(nameof(item.UkPrn), "UkPrn has not been supplied");
        }

        //TODO: When we sort out how to handle null course names add this back
        //if (string.IsNullOrEmpty(item.CourseName))
        //{
        //    validationResult.AddError(nameof(item.CourseName), "Course name has not been supplied");
        //}

        if (item.FromDate == DateTime.MinValue)
        {
            validationResult.AddError(nameof(item.FromDate), "From date has not been supplied");
        }

        if (item.ToDate == DateTime.MinValue)
        {
            validationResult.AddError(nameof(item.ToDate), "To date has not been supplied");
        }
        
        return Task.FromResult(validationResult);
    }
}