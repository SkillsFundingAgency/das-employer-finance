using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;

public class TransferStagedToOperationalCommandValidator : IValidator<TransferStagedToOperationalCommand>
{
    public ValidationResult Validate(TransferStagedToOperationalCommand command)
    {
        var result = new ValidationResult();

        if (command.AccountId <= 0)
        {
            result.AddError(nameof(command.AccountId), "AccountId must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(command.PeriodEnd))
        {
            result.AddError(nameof(command.PeriodEnd), "PeriodEnd is required");
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(TransferStagedToOperationalCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}
