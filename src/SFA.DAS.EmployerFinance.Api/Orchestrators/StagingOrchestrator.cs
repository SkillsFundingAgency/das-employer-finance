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

    public async Task<IActionResult> IngestBulkPayments(List<PaymentStagingModel> payments)
    {
        var correlationId = Guid.NewGuid().ToString();
        var receivedCount = payments?.Count ?? 0;

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Bulk payments ingestion request received. Received={received}",
            correlationId,
            receivedCount);

        if (payments == null)
        {
            _logger.LogWarning(
                "[CorrelationId: {CorrelationId}] Bulk payments ingestion failed. " +
                "Received=0, Accepted=0, Rejected=0. Request body is null.",
                correlationId);

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed.",
                Errors = new[] { "Request body is required." }
            });
        }

        var command = new BulkPaymentsIngestCommand
        {
            Payments = payments,
            CorrelationId = correlationId
        };

        var validationResult = await new BulkPaymentsIngestCommandValidator()
            .ValidateAsync(command);

        if (!validationResult.IsValid())
        {
            _logger.LogWarning(
                "[CorrelationId: {CorrelationId}] Bulk payments ingestion validation failed. " +
                "Received={received}, Accepted=0, Rejected={rejected}, Errors={errors}",
                correlationId,
                receivedCount,
                receivedCount,
                string.Join(", ", validationResult.ErrorList));

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed.",
                Errors = validationResult.ErrorList
            });
        }

        var response = await _mediator.Send(command);

        if (!response.IsSuccess)
        {
            _logger.LogWarning(
                "[CorrelationId: {CorrelationId}] Bulk payments ingestion failed. " +
                "Received={received}, Accepted=0, Rejected={rejected}, Message={message}",
                correlationId,
                receivedCount,
                receivedCount,
                response.Message);

            return new BadRequestObjectResult(new
            {
                Message = response.Message
            });
        }

        _logger.LogInformation(
            "[CorrelationId: {CorrelationId}] Bulk payments ingestion succeeded. " +
            "Received={received}, Accepted={accepted}, Rejected={rejected}",
            correlationId,
            receivedCount,
            response.InsertedCount,
            receivedCount - response.InsertedCount);

        return new CreatedResult(string.Empty, new
        {
            InsertedCount = response.InsertedCount,
            PaymentIds = response.PaymentIds
        });
    }



}