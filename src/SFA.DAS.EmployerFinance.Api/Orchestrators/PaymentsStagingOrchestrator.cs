using SFA.DAS.EmployerFinance.Commands.BulkPaymentsIngest;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Orchestrators;

public class PaymentsStagingOrchestrator
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsStagingOrchestrator> _logger;

    public PaymentsStagingOrchestrator(IMediator mediator, ILogger<PaymentsStagingOrchestrator> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<IActionResult> IngestBulkPayments(List<PaymentStagingModel> payments)
    {
        var correlationId = Guid.NewGuid().ToString();
        _logger.LogInformation("[CorrelationId: {CorrelationId}] Requesting payment records for bulk payment ingestion", correlationId);

        var command = new BulkPaymentsIngestCommand
        {
            Payments = payments,
            CorrelationId = correlationId
        };
        //manually validating the command here to send 400 Bad Request response on validation errors
        var validationResult = await new BulkPaymentsIngestCommandValidator().ValidateAsync(command);
        if (!validationResult.IsValid())
        {
            _logger.LogWarning("[CorrelationId: {CorrelationId}] Bulk payments ingestion Validation Failed: Errors = {errors}", correlationId, string.Join(", ", validationResult.ErrorList));
            return new BadRequestObjectResult(new { Message = "Validation failed.", Errors = validationResult.ErrorList });
        }

        var response = await _mediator.Send(command);

        if (response.IsSuccess == false)
        {
            _logger.LogWarning("[CorrelationId: {CorrelationId}] Bulk payments ingestion failed: Message = {message}", correlationId, response.Message);
            return new BadRequestObjectResult(new { Message = response.Message });
        }

        _logger.LogInformation("[CorrelationId: {CorrelationId}] Received bulk payments ingestion response: IsSuccess = {isSuccess}, Message = {message}, InsertedCount = {insertedCount}", correlationId, response.IsSuccess, response.Message, response.InsertedCount);
        return new CreatedResult(string.Empty, new
        {
            InserteCount = response.InsertedCount,
            PaymentIds = response.PaymentIds
        });
    }
}