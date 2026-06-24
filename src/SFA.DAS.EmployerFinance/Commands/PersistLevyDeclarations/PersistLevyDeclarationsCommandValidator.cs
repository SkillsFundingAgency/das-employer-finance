using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.PersistLevyDeclarations;

public class PersistLevyDeclarationsCommandValidator : IValidator<PersistLevyDeclarationsCommand>
{
    public ValidationResult Validate(PersistLevyDeclarationsCommand item)
    {
        var validationResult = new ValidationResult();

        if (item.Data == null)
        {
            validationResult.AddError(nameof(item.Data), "Request payload is required.");
            return validationResult;
        }

        if (item.Data.AccountId <= 0)
        {
            validationResult.AddError(nameof(item.Data.AccountId), "AccountId must be supplied and greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(item.Data.EmpRef))
        {
            validationResult.AddError(nameof(item.Data.EmpRef), "EmpRef has not been supplied.");
        }

        if (item.Data.Declarations == null || item.Data.Declarations.Count == 0)
        {
            validationResult.AddError(nameof(item.Data.Declarations), "A non-empty Declarations collection is required.");
            return validationResult;
        }

        for (var i = 0; i < item.Data.Declarations.Count; i++)
        {
            var d = item.Data.Declarations[i];
            var prefix = $"{nameof(item.Data.Declarations)}[{i}]";

            if (d == null)
            {
                validationResult.AddError(prefix, "Declaration entry is required.");
                continue;
            }

            if (d.Id <= 0)
            {
                validationResult.AddError($"{prefix}.{nameof(d.Id)}", "Id must be greater than zero.");
            }

            if (d.SubmissionId <= 0)
            {
                validationResult.AddError($"{prefix}.{nameof(d.SubmissionId)}", "SubmissionId must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(d.PayrollYear))
            {
                validationResult.AddError($"{prefix}.{nameof(d.PayrollYear)}", "PayrollYear has not been supplied.");
            }

            if (!d.PayrollMonth.HasValue)
            {
                validationResult.AddError($"{prefix}.{nameof(d.PayrollMonth)}", "PayrollMonth has not been supplied.");
            }

            if (d.SubmissionDate == DateTime.MinValue)
            {
                validationResult.AddError($"{prefix}.{nameof(d.SubmissionDate)}", "SubmissionDate has not been supplied.");
            }
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(PersistLevyDeclarationsCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}
