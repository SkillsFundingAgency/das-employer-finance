using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;


[Route("api/payments")]
public class PaymentController(PaymentOrchestrator paymentOrchestrator) : ControllerBase
{
    [Authorize]
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentMetaDataStaging(Guid paymentId, [FromBody] PaymentMetaDataStaging request)
    {
        if (request is null || paymentId == Guid.Empty)
            return BadRequest();

        try
        {
            var updatePaymentMetadataStaging =
                await paymentOrchestrator.UpdatePaymentMetaDataStaging(paymentId, request);

            if (updatePaymentMetadataStaging == null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Could not update Payment Metadata Staging");

            return Ok(updatePaymentMetadataStaging);
        }
        catch (Exception ex)
        {
            return BadRequest("Could not update Payment Metadata Staging");
        }
    }
}
