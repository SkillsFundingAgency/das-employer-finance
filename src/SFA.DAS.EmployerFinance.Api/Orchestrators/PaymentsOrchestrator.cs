using AutoMapper;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Commands.CreateNewPeriodEnd;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentsOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsOrchestrator> _logger;
    private readonly IMapper _mapper;

    public PaymentsOrchestrator
    (
        IMediator mediator,
        ILogger<PaymentsOrchestrator> logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Payment> PaymentsMetadata(Payment periodEnd)
    {
        _logger.LogInformation("Requesting period end save with period end id {PeriodEndId}", periodEnd.PeriodEndId);
        var mappedPeriodEnd = _mapper.Map<PeriodEnd>(periodEnd);

        await _mediator.Send(new CreateNewPeriodEndCommand { NewPeriodEnd = mappedPeriodEnd });

        var created = await GetPeriodEndByPeriodEndId(periodEnd.PeriodEndId);
        if (created == null)
        {
            _logger.LogError("Period end {PeriodEndId} was not created", periodEnd.PeriodEndId);
            return null;
        }

        return _mapper.Map<Types.PeriodEnd>(mappedPeriodEnd);
    }
}
