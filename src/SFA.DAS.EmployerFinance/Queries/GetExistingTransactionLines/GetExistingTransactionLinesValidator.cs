using SFA.DAS.EmployerFinance.Models.Transaction;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Queries.GetExistingTransactionLines;

public class GetExistingTransactionLinesValidator : IValidator<GetExistingTransactionLinesQuery>
{
    public ValidationResult Validate(GetExistingTransactionLinesQuery item)
    {
        throw new NotImplementedException();
    }

    public Task<ValidationResult> ValidateAsync(GetExistingTransactionLinesQuery item)
    {
        var result = new ValidationResult();

        if (string.IsNullOrEmpty(item.HashedAccountId))
        {
            result.AddError(nameof(item.HashedAccountId), "HashedAccountId has not been supplied");
        }

        if (string.IsNullOrWhiteSpace(item.PeriodEnd))
        {
            result.AddError(nameof(item.PeriodEnd), "PeriodEnd has not been supplied");
        }

        if (item.TransactionType != (int)TransactionItemType.Payment)
        {
            result.AddError(nameof(item.TransactionType), "Only payment transaction type (3) is supported");
        }

        return Task.FromResult(result);
    }
}
