using AutoMapper;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.EmployerFinance.Queries.GetPeriodEnds;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PeriodEndOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<PeriodEndOrchestrator> _logger;
    private readonly IMapper _mapper;

    public PeriodEndOrchestrator(
        IMediator mediator,
        ILogger<PeriodEndOrchestrator> logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<List<Types.PeriodEnd>> GetPeriodEnds()
    {
        _logger.LogInformation("Requesting all period ends");

        var response = await _mediator.Send(new GetPeriodEndsRequest());
        if (response.CurrentPeriodEnds == null)
        {
            return null;
        }

        var periodEnds = response.CurrentPeriodEnds.Select(x => _mapper.Map<Types.PeriodEnd>(x)).ToList();

        _logger.LogInformation("Received response for Get All Period ends");

        return periodEnds;
    }

    public async Task<Types.PeriodEnd> GetPeriodEndByPeriodEndId(string periodEndId)
    {
        _logger.LogInformation("Requesting period ends by id");

        var response = await _mediator.Send(new GetPeriodEndByPeriodEndIdRequest { PeriodEndId = periodEndId });
        if (response.PeriodEnd == null)
        {
            return null;
        }

        var periodEnd = _mapper.Map<Types.PeriodEnd>(response.PeriodEnd);

        _logger.LogInformation("Received response for period ends by period end id {PeriodEndId}", periodEndId);

        return periodEnd;
    }

    public async Task<Types.PeriodEnd> CreatePeriodEnd(Types.PeriodEnd periodEnd)
    {
        _logger.LogInformation("Requesting period end save with period end id {PeriodEndId}", periodEnd.PeriodEndId);
        var mappedPeriodEnd = _mapper.Map<PeriodEnd>(periodEnd);

        await _mediator.Send(new CreateNewPeriodEndCommand { NewPeriodEnd = mappedPeriodEnd });

        var created = GetPeriodEndByPeriodEndId(periodEnd.PeriodEndId);
        if (created == null)
        {
            _logger.LogError("Period end {PeriodEndId} was not created", periodEnd.PeriodEndId);
            return null;
        }

        return _mapper.Map<Types.PeriodEnd>(mappedPeriodEnd);
    }
}
