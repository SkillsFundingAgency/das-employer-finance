﻿using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

public class CreateEnglishFractionCalculationDateCommandHandler : IRequestHandler<CreateEnglishFractionCalculationDateCommand>
{
    private readonly IValidator<CreateEnglishFractionCalculationDateCommand> _validator;
    private readonly IEnglishFractionRepository _englishFractionRepository;
    private readonly ILogger<CreateEnglishFractionCalculationDateCommandHandler> _logger;

    public CreateEnglishFractionCalculationDateCommandHandler(
        IValidator<CreateEnglishFractionCalculationDateCommand> validator,
        IEnglishFractionRepository englishFractionRepository,
        ILogger<CreateEnglishFractionCalculationDateCommandHandler> logger)
    {
        _validator = validator;
        _englishFractionRepository = englishFractionRepository;
        _logger = logger;
    }

    public async Task Handle(CreateEnglishFractionCalculationDateCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        await _englishFractionRepository.SetLastUpdateDate(request.DateCalculated);

        _logger.LogInformation("English Fraction CalculationDate updated to {DateCalculated}.", request.DateCalculated.ToString("dd MMM yyyy"));
    }
}