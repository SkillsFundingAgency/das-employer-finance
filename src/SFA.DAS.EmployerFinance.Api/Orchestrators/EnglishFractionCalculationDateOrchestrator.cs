using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class EnglishFractionCalculationDateOrchestrator(
    IMediator mediator,
    ILogger<EnglishFractionCalculationDateOrchestrator> logger)
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<EnglishFractionCalculationDateOrchestrator> _logger = logger;

    public async Task PersistCalculationDate(EnglishFractionCalculationDateRequest request)
    {
        _logger.LogInformation(
            "Persisting english fraction calculation date {DateCalculated}",
            request.DateCalculated);

        await _mediator.Send(new CreateEnglishFractionCalculationDateCommand
        {
            DateCalculated = request.DateCalculated
        });
    }
}