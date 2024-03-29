﻿namespace SFA.DAS.EmployerFinance.Validation;

public interface IValidator<T>
{
    ValidationResult Validate(T item);
    Task<ValidationResult> ValidateAsync(T item);
}