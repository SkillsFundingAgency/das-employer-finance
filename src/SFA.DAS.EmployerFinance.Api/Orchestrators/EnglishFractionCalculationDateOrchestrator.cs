using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateEnglishFractionCalculationDate;
using SFA.DAS.EmployerFinance.Queries.GetLastEnglishFractionCalculationDate;
namespace SFA.DAS.EmployerFinance.Api.Orchestrators;
public class EnglishFractionCalculationDateOrchestrator(
    IMediator mediator,
    ILogger<EnglishFractionCalculationDateOrchestrator> logger)
{

    private readonly IMediator _mediator = mediator;

    private readonly ILogger<EnglishFractionCalculationDateOrchestrator> _logger = logger;

    public async Task<LastEnglishFractionCalculationDateResult> GetLastCalculationDate(string empRef)

    {

        _logger.LogInformation("Requesting last stored english fraction calculation date for empRef {EmpRef}", empRef);

        var response = await _mediator.Send(new GetLastEnglishFractionCalculationDateQuery

        {

            EmpRef = empRef

        });

        _logger.LogInformation("Received last stored english fraction calculation date for empRef {EmpRef}", empRef);

        return new LastEnglishFractionCalculationDateResult

        {

            DateCalculated = response.DateCalculated

        };

    }

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
