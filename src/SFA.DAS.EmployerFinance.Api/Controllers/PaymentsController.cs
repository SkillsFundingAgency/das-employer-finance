using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentsController(StagingOrchestrator orchestrator)
    : ControllerBase
{
    [Route("staging", Name = "BulkPaymentsIngest")]
    [HttpPost]
    public async Task<IActionResult> BulkPaymentsIngest([FromBody] BulkPaymentsRequest request)
    {
        if (request?.Payments == null || request.Payments.Count == 0)
            return BadRequest("Payments payload is required.");

        var response = await orchestrator.IngestBulkPayments(request);

        if (response.HasValidationErrors)
            return BadRequest(response.ValidationErrors);

        if (response.ConflictingPaymentIds.Count > 0)
            return Conflict(new { paymentIds = response.ConflictingPaymentIds });

        if (!response.IsSuccess)
            return StatusCode(500, "Unexpected error staging payments.");

        return StatusCode(201, new
        {
            insertedCount = response.InsertedCount,
            paymentIds = response.PaymentIds
        });
    }
}