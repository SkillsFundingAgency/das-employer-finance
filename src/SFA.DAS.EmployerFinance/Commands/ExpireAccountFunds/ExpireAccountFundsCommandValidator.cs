using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.ExpireAccountFunds;

public class ExpireAccountFundsCommandValidator : IValidator<ExpireAccountFundsCommand>
{
    public ValidationResult Validate(ExpireAccountFundsCommand command)
    {
        var result = new ValidationResult();

        if (command.AccountId <= 0)
        {
            result.AddError(nameof(command.AccountId), "AccountId must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(command.CorrelationId))
        {
            result.AddError(nameof(command.CorrelationId), "CorrelationId is required.");
        }
        else if (command.CorrelationId.Length > 100)
        {
            result.AddError(nameof(command.CorrelationId), "CorrelationId must be 100 characters or fewer.");
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(ExpireAccountFundsCommand command)
    {
        return Task.FromResult(Validate(command));
    }
}
