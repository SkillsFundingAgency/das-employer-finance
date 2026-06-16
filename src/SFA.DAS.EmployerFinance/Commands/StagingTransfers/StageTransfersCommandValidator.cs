using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.StagingTransfers;

public class StageTransfersCommandValidator : IValidator<StageTransfersCommand>
{
    public ValidationResult Validate(StageTransfersCommand command)
    {
        var result = new ValidationResult();

        if (command.Transfers == null || !command.Transfers.Any())
            result.AddError(nameof(command.Transfers), "Transfers array must contain 1–1000 items");

        if (command.Transfers.Count > 1000)
            result.AddError(nameof(command.Transfers), "Transfers array cannot exceed 1000 items");

        foreach (var t in command.Transfers)
        {
            if (t.TransferId == 0)
                result.AddError(nameof(t.TransferId), "TransferId is required");

            if (t.TransferId <= 0)
                result.AddError(nameof(t.TransferId), "TransferId must be greater than 0");

            if (t.Amount <= 0)
                result.AddError(nameof(t.Amount), "Amount must be greater than 0");

            if (t.SenderAccountId == 0)
                result.AddError(nameof(t.SenderAccountId), "SenderAccountId is required");

            if (t.ReceiverAccountId == 0)
                result.AddError(nameof(t.ReceiverAccountId), "ReceiverAccountId is required");

            if (t.ReceiverAccountId <= 0)
                result.AddError(nameof(t.ReceiverAccountId), "ReceiverAccountId must be greater than 0");

            if (string.IsNullOrWhiteSpace(t.PeriodEnd))
                result.AddError(nameof(t.PeriodEnd), "PeriodEnd is required");
        }

        return result;
    }

    public Task<ValidationResult> ValidateAsync(StageTransfersCommand item)
    {
        return Task.FromResult(Validate(item));
    }
}