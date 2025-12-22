using AutoMapper;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class TransferOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferOrchestrator> _logger;
    private readonly IMapper _mapper;

    public TransferOrchestrator(
        IMediator mediator,
        ILogger<TransferOrchestrator> logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<GetTransfersByPeriodEndResponse> GetTransfersByPeriodEnd(long accountId, string periodEndId)
    {
        return await _mediator.Send(new GetTransfersByPeriodEndRequest { AccountId = accountId, PeriodEnd = periodEndId });
    }
}
