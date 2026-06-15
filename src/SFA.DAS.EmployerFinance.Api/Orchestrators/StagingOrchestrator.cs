using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class StagingOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<StagingOrchestrator> _logger;

    public StagingOrchestrator(IMediator mediator, ILogger<StagingOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<BulkPaymentsIngestResponse> IngestBulkPayments(BulkPaymentsRequest request)
    {
        _logger.LogInformation("Staging {Count} payments", request.Payments?.Count ?? 0);

        return await _mediator.Send(new BulkPaymentsIngestCommand
        {
            Payments = request.Payments
        });
    }
}