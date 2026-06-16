using SFA.DAS.EmployerFinance.Commands.TransactionLineStaging;
using SFA.DAS.EmployerFinance.Models.TransactionLineStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class TransactionLineStagingOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionLineStagingOrchestrator> _logger;

    public TransactionLineStagingOrchestrator(IMediator mediator, ILogger<TransactionLineStagingOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<TransactionLineStagingResponse> IngestTransactionLines(TransactionLineStagingRequest request)
    {
        _logger.LogInformation("Staging {Count} transaction lines", request.TransactionLines?.Count ?? 0);

        return await _mediator.Send(new TransactionLineStagingCommand
        {
            TransactionLines = request.TransactionLines
        });
    }
}