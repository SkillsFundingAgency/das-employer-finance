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

    public async Task<BulkTransferStagingResponse> StageTransfers(BulkTransferStagingRequest request)
    {
        _logger.LogInformation("Staging {Count} transfers", request.Transfers?.Count ?? 0);

        var command = _mapper.Map<StageTransfersCommand>(request);
        var result = await _mediator.Send(command);

        var response = _mapper.Map<BulkTransferStagingResponse>(result);
        return response;
    }
}
