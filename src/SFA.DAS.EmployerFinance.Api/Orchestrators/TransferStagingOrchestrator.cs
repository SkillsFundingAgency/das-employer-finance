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

    public async Task<StageTransfersResponse> StageTransfers(StageTransfersRequest request)
    {
        _logger.LogInformation("Staging {Count} transfers", request.Transfers?.Count ?? 0);

        return await _mediator.Send(new StageTransfersCommand { Transfers = request.Transfers });
    }
}