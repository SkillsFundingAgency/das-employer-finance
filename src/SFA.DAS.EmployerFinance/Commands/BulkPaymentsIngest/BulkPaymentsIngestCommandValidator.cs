using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestCommandValidator : IValidator<BulkPaymentsIngestCommand>
    {
        public ValidationResult Validate(BulkPaymentsIngestCommand command)
        {
            var result = new ValidationResult();

            if (command.Payments == null || command.Payments.Count == 0)
            {
                result.AddError(nameof(command.Payments), "Payments array is required and cannot be empty.");
                return result;
            }

            if (command.Payments.Count > 1000)
            {
                result.AddError(nameof(command.Payments), "Payments batch size exceeds the limit of 1000 items.");
                return result;
            }

            for (int i = 0; i < command.Payments.Count; i++)
            {
                var item = command.Payments[i];
                string prefix = $"Payments[{i}]";

                if (item.PaymentId == Guid.Empty) result.AddError($"{prefix}.PaymentId", "PaymentId is mandatory.");
                if (item.AccountId <= 0) result.AddError($"{prefix}.AccountId", "AccountId is mandatory and must be > 0.");
                if (string.IsNullOrWhiteSpace(item.CollectionPeriodId)) result.AddError($"{prefix}.CollectionPeriodId", "CollectionPeriodId is mandatory.");
            }

            return result;
        }

        public Task<ValidationResult> ValidateAsync(BulkPaymentsIngestCommand item) => Task.FromResult(Validate(item));

    }
}
