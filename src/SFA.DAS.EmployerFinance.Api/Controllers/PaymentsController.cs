using Azure;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentsController(StagingOrchestrator stagingOrchestrator,
    PaymentOrchestrator paymentOrchestrator)
    : ControllerBase
{
    [Route("staging", Name = "BulkPaymentsIngest")]
    [HttpPost]
    public async Task<IActionResult> BulkPaymentsIngest([FromBody] BulkPaymentsRequest request)
    {
        if (request?.Payments == null || request.Payments.Count == 0)
            return BadRequest("Payments payload is required.");

        var response = await stagingOrchestrator.IngestBulkPayments(request);

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

    [HttpGet]
    [Route($"account/{{accountId:long}}/apprenticeship/{{apprenticeshipId:long}}", Name = "GetPaymentsByAccountId")]
    public async Task<IActionResult> GetPaymentsByAccountId([FromRoute] long accountId,
        [FromRoute] long apprenticeshipId,
        CancellationToken cancellationToken)
    {
        var payments = await paymentOrchestrator.GetPayments(accountId, apprenticeshipId, cancellationToken);
        return StatusCode(200, new
        {
            Payments = payments
        });
    }
}