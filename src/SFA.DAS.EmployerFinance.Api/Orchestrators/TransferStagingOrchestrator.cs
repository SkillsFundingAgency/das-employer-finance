using AutoMapper;
using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class TransferStagingOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransferStagingOrchestrator> _logger;
    private readonly IMapper _mapper;

    public TransferStagingOrchestrator(
        IMediator mediator,
        ILogger<TransferStagingOrchestrator> logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
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