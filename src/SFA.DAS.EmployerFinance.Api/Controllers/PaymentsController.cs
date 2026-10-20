using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerFinance.Api.Types;

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
    [Route($"account/{{accountId:long}}/apprenticeship/{{apprenticeshipId:long}}", Name = "GetPaymentDetails")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentDetails([FromRoute] long accountId,
        [FromRoute] long apprenticeshipId,
        CancellationToken cancellationToken)
    {
        var payments = await paymentOrchestrator.GetPaymentDetails(accountId, apprenticeshipId, cancellationToken);
        return Ok(payments);
    }

    [HttpGet]
    [Route("account/{accountId:long}/funds-out", Name = "GetPaymentFundsOut")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [ProducesResponseType(typeof(List<CalendarPeriodAmount>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentFundsOutForCalendarPeriods([FromRoute] long accountId, CancellationToken cancellationToken)
    {
        var result = await paymentOrchestrator.GetPaymentFundsOut(accountId, cancellationToken);
        return Ok(result);
    }
}