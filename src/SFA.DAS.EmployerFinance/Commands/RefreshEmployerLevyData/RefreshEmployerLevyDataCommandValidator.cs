﻿using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.RefreshEmployerLevyData;

public class RefreshEmployerLevyDataCommandValidator : IValidator<RefreshEmployerLevyDataCommand>
{
    public ValidationResult Validate(RefreshEmployerLevyDataCommand item)
    {
        //TODO VALIDATE!!!
        var validationResult = new ValidationResult();

        return validationResult;
    }

    public Task<ValidationResult> ValidateAsync(RefreshEmployerLevyDataCommand item)
    {
        throw new NotImplementedException();
    }
}