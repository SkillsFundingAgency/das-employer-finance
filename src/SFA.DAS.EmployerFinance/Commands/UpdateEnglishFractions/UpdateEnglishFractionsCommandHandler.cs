using SFA.DAS.EmployerFinance.Data.Contracts;
using SFA.DAS.EmployerFinance.Interfaces.Hmrc;
using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Commands.UpdateEnglishFractions;

public class UpdateEnglishFractionsCommandHandler : IRequestHandler<UpdateEnglishFractionsCommand>
{
    private readonly IHmrcService _hmrcService;
    private readonly IEnglishFractionRepository _englishFractionRepository;
    private readonly ILogger<UpdateEnglishFractionsCommandHandler> _logger;

    public UpdateEnglishFractionsCommandHandler(IHmrcService hmrcService,
        IEnglishFractionRepository englishFractionRepository,
        ILogger<UpdateEnglishFractionsCommandHandler> logger)
    {
        _hmrcService = hmrcService;
        _englishFractionRepository = englishFractionRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateEnglishFractionsCommand request, CancellationToken cancellationToken)
    {
        var existingFractions = (await _englishFractionRepository.GetAllEmployerFractions(request.EmployerReference)).ToList();

        if (existingFractions.Any() && !request.EnglishFractionUpdateResponse.UpdateRequired && TheFractionIsOlderOrEqualToTheUpdateDate(request, existingFractions))
        {
            return;   
        }

        DateTime? dateFrom = null;
        if (existingFractions.MaxBy(x => x.DateCalculated)?.DateCalculated != null
            && existingFractions.MaxBy(x => x.DateCalculated)?.DateCalculated != DateTime.MinValue)
        {
            dateFrom = existingFractions.MaxBy(x => x.DateCalculated)?.DateCalculated.AddDays(-1);
        }


        var fractionCalculations = await _hmrcService.GetEnglishFractions(request.EmployerReference, dateFrom);

        if (fractionCalculations?.FractionCalculations == null)
        {
            return;
        }

        var hmrcFractions = fractionCalculations.FractionCalculations.SelectMany(calculations =>
        {
            var fractions = new List<DasEnglishFraction>();

            foreach (var fraction in calculations.Fractions)
            {
                if (decimal.TryParse(fraction.Value, out _))
                {
                    fractions.Add(
                        new DasEnglishFraction
                        {
                            EmpRef = fractionCalculations.Empref,
                            DateCalculated = calculations.CalculatedAt,
                            Amount = decimal.Parse(fraction.Value)
                        });
                }
                else
                {
                    var exception = new InvalidOperationException($"Could not convert HMRC API fraction value {fraction.Value} to a decimal for english fraction update for EmpRef {request.EmployerReference}");
                    _logger.LogError(exception, exception.Message);
                }
            }

            return fractions;
        }).ToList();

        var newFraction = hmrcFractions.Except(existingFractions, new DasEmployerComparer()).ToList();

        foreach (var englishFraction in newFraction)
        {
            await _englishFractionRepository.CreateEmployerFraction(englishFraction, englishFraction.EmpRef);
        }
    }

    private static bool TheFractionIsOlderOrEqualToTheUpdateDate(UpdateEnglishFractionsCommand message, List<DasEnglishFraction> existingFractions)
    {
        return message.EnglishFractionUpdateResponse.DateCalculated <=
               existingFractions.OrderByDescending(x => x.DateCalculated).First().DateCalculated;
    }
}

public class DasEmployerComparer : IEqualityComparer<DasEnglishFraction>
{
    public bool Equals(DasEnglishFraction source, DasEnglishFraction target)
    {
        return source.EmpRef.Equals(target?.EmpRef) &&
               source.DateCalculated.Equals(target?.DateCalculated);
    }

    public int GetHashCode(DasEnglishFraction obj)
    {
        return obj.DateCalculated.GetHashCode() ^ obj.EmpRef.GetHashCode();
    }
}