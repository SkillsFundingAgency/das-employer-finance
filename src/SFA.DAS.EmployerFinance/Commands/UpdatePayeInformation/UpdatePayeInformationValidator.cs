﻿using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.UpdatePayeInformation;

public class UpdatePayeInformationValidator : IValidator<UpdatePayeInformationCommand>
{
    public ValidationResult Validate(UpdatePayeInformationCommand item)
    {
        var validationResult = new ValidationResult();
        if (string.IsNullOrEmpty(item.PayeRef))
        {
            validationResult.AddError(nameof(item.PayeRef));
        }

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(UpdatePayeInformationCommand item)
    {
        throw new NotImplementedException();
    }
}