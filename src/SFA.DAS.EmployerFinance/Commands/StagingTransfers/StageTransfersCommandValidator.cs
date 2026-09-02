using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersCommandValidator : IValidator<StageTransfersCommand>
{
    public ValidationResult Validate(StageTransfersCommand command)
    {
        var result = new ValidationResult();

        if (command.Transfers == null || !command.Transfers.Any())
        {
            result.AddError(nameof(command.Transfers), "Transfers array must contain 1-1000 items");
            return result;
        }

        if (command.Transfers.Count > 1000)
        {
            result.AddError(nameof(command.Transfers), "Transfers array cannot exceed 1000 items");
            return result;
        }

        for (var i = 0; i < command.Transfers.Count; i++)
        {
            var t = command.Transfers[i];
            var prefix = $"Transfers[{i}]";

            if (t.TransferId <= 0)
                result.AddError($"{prefix}.TransferId", "TransferId must be greater than 0");

            if (t.SenderAccountId <= 0)
                result.AddError($"{prefix}.SenderAccountId", "SenderAccountId is required");

            if (t.ReceiverAccountId <= 0)
                result.AddError($"{prefix}.ReceiverAccountId", "ReceiverAccountId must be greater than 0");

            if (string.IsNullOrWhiteSpace(t.PeriodEnd))
                result.AddError($"{prefix}.PeriodEnd", "PeriodEnd is required");

            if (string.IsNullOrWhiteSpace(t.Type))
                result.AddError($"{prefix}.Type", "Type is required");
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(StageTransfersCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}
