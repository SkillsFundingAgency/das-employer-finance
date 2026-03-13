using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentMetaDataController(PaymentMetaDataOrchestrator paymentOrchestrator) : ControllerBase
{
    [Authorize]
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentMetaDataStaging(Guid paymentId, [FromBody] PaymentMetaDataStaging request)
    {
        if (request == null)
            return BadRequest("Request body is required.");

        var response = await paymentOrchestrator.UpdatePaymentMetaDataStaging(paymentId, request);

        if (response.HasValidationErrors)
            return BadRequest(response.ValidationErrors);

        if (response.NotFound)
            return NotFound();

        if (!response.IsSuccess)
            return StatusCode(500, "Could not update Payment Metadata Staging");

        return Ok(new
        {
            upserted = response.Upserted,
            metadataId = response.MetadataId
        });
    }
}