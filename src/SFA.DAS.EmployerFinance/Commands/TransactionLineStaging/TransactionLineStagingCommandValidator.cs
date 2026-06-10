using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;

public class TransactionLineStagingCommandValidator : IValidator<TransactionLineStagingCommand>
{
    public ValidationResult Validate(TransactionLineStagingCommand command)
    {
        var result = new ValidationResult();

        if (command.TransactionLines == null || command.TransactionLines.Count == 0)
        {
            result.AddError(nameof(command.TransactionLines), "TransactionLines array is required and cannot be empty.");
            return result;
        }

        if (command.TransactionLines.Count > 1000)
        {
            result.AddError(nameof(command.TransactionLines), "TransactionLines batch size exceeds the limit of 1000 items.");
            return result;
        }

        for (var i = 0; i < command.TransactionLines.Count; i++)
        {
            var item = command.TransactionLines[i];
            var prefix = $"TransactionLines[{i}]";

            if (item.AccountId <= 0)
                result.AddError($"{prefix}.AccountId", "AccountId is mandatory and must be > 0.");

            if (item.TransactionDate == default)
                result.AddError($"{prefix}.TransactionDate", "TransactionDate is mandatory.");

            if (item.TransactionType < 0)
                result.AddError($"{prefix}.TransactionType", "TransactionType is not a valid value.");

            if (item.TransactionType > byte.MaxValue)
                result.AddError($"{prefix}.TransactionType", "TransactionType is not a valid value.");
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(TransactionLineStagingCommand item) => Task.FromResult(Validate(item));
}
