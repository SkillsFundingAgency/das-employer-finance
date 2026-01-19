using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest
{
    public class BulkPaymentsIngestCommandValidator : IValidator<BulkPaymentsIngestCommand>
    {
        public ValidationResult Validate(BulkPaymentsIngestCommand command)
        {
            var result = new ValidationResult();

            // 1. Array Level Validation (1–1000 items)
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

            // To track duplicates within the batch
            var seenPaymentIds = new HashSet<Guid>();

            // 2. Individual Item Validation
            for (int i = 0; i < command.Payments.Count; i++)
            {
                var item = command.Payments[i];
                string prefix = $"Payments[{i}]";

                // Duplicate Check
                if (item.PaymentId != Guid.Empty && !seenPaymentIds.Add(item.PaymentId))
                {
                    result.AddError($"{prefix}.PaymentId", $"Duplicate PaymentId '{item.PaymentId}' detected within this batch.");
                }

                // Mandatory IDs
                if (item.PaymentId == Guid.Empty) result.AddError($"{prefix}.PaymentId", "PaymentId is mandatory.");
                if (item.AccountId <= 0) result.AddError($"{prefix}.AccountId", "AccountId is mandatory and must be > 0.");
                if (item.Ukprn <= 0) result.AddError($"{prefix}.Ukprn", "Ukprn is mandatory and must be > 0.");
                if (item.Uln <= 0) result.AddError($"{prefix}.Uln", "Uln is mandatory and must be > 0.");
                if (item.ApprenticeshipId <= 0) result.AddError($"{prefix}.ApprenticeshipId", "ApprenticeshipId is mandatory and must be > 0.");

                // Mandatory Period Fields
                if (string.IsNullOrWhiteSpace(item.CollectionPeriodId)) result.AddError($"{prefix}.CollectionPeriodId", "CollectionPeriodId is mandatory.");
                if (item.DeliveryPeriodMonth < 1 || item.DeliveryPeriodMonth > 12) result.AddError($"{prefix}.DeliveryPeriodMonth", "DeliveryPeriodMonth must be between 1 and 12.");
                if (item.DeliveryPeriodYear <= 0) result.AddError($"{prefix}.DeliveryPeriodYear", "DeliveryPeriodYear is mandatory.");
                if (item.CollectionPeriodMonth < 1 || item.CollectionPeriodMonth > 12) result.AddError($"{prefix}.CollectionPeriodMonth", "CollectionPeriodMonth must be between 1 and 12.");
                if (item.CollectionPeriodYear <= 0) result.AddError($"{prefix}.CollectionPeriodYear", "CollectionPeriodYear is mandatory.");

                // Financial Validation
                if (item.Amount < 0) result.AddError($"{prefix}.Amount", "Amount must be non-negative.");
            }

            return result;
        }

        public Task<ValidationResult> ValidateAsync(BulkPaymentsIngestCommand item) => Task.FromResult(Validate(item));

    }
}