using System.ComponentModel.DataAnnotations;
using SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;
using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.Commands.PersistEnglishFractions;

public class PersistEnglishFractionsCommandHandler : IRequestHandler<PersistEnglishFractionsCommand, PersistEnglishFractionsResponse>
{
    private readonly IEnglishFractionRepository _englishFractionRepository;
    private readonly ILogger<PersistEnglishFractionsCommandHandler> _logger;
    private readonly IValidator<PersistEnglishFractionsCommand> _validator;

    public PersistEnglishFractionsCommandHandler(
        IEnglishFractionRepository englishFractionRepository,
        ILogger<PersistEnglishFractionsCommandHandler> logger,
        IValidator<PersistEnglishFractionsCommand> validator)
    {
        _englishFractionRepository = englishFractionRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<PersistEnglishFractionsResponse> Handle(PersistEnglishFractionsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = _validator.Validate(request);

        if (!validationResult.IsValid())
        {
            throw new ValidationException(validationResult.ConvertToDataAnnotationsValidationResult(), null, null);
        }

        var existingFractions = (await _englishFractionRepository.GetAllEmployerFractions(request.EmployerReference)).ToList();
        var incomingFractions = request.Fractions ?? new List<Models.Levy.DasEnglishFraction>();

        if (existingFractions.Any() && !request.UpdateRequired && TheFractionIsOlderOrEqualToTheUpdateDate(request, existingFractions))
        {
            _logger.LogInformation("No english fraction update required for empRef {EmpRef}", request.EmployerReference);
            return new PersistEnglishFractionsResponse
            {
                Stored = 0,
                Ignored = incomingFractions.Count
            };
        }

        if (!incomingFractions.Any())
        {
            return new PersistEnglishFractionsResponse { Stored = 0, Ignored = 0 };
        }

        var newFractions = incomingFractions.Except(existingFractions, new DasEmployerComparer()).ToList();

        foreach (var englishFraction in newFractions)
        {
            await _englishFractionRepository.CreateEmployerFraction(englishFraction, englishFraction.EmpRef);
        }

        return new PersistEnglishFractionsResponse
        {
            Stored = newFractions.Count,
            Ignored = incomingFractions.Count - newFractions.Count
        };
    }

    private static bool TheFractionIsOlderOrEqualToTheUpdateDate(PersistEnglishFractionsCommand message, List<Models.Levy.DasEnglishFraction> existingFractions)
    {
        return message.DateCalculated <= existingFractions.OrderByDescending(x => x.DateCalculated).First().DateCalculated;
    }
}
