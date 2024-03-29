﻿using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshPaymentData;

public class RefreshPaymentDataCommandValidator : IValidator<RefreshPaymentDataCommand>
{
    public ValidationResult Validate(RefreshPaymentDataCommand item)
    {
        var validationResult = new ValidationResult();

        if (item.AccountId == 0)
        {
            validationResult.AddError(nameof(item.AccountId), "AccountId has not been supplied");
        }

        if (string.IsNullOrEmpty(item.PeriodEnd))
        {
            validationResult.AddError(nameof(item.PeriodEnd), "PeriodEnd has not been supplied");
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(RefreshPaymentDataCommand item)
    {
        throw new System.NotImplementedException();
    }
}