using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.Payments;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentController(PaymentOrchestrator paymentOrchestrator) : ControllerBase
{
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentMetadata(Guid paymentId, [FromBody] PaymentMetaData request)
    {
        if (request is null || paymentId == Guid.Empty)
            return BadRequest();

        try
        {
            var updatePaymentMetadata =
                await paymentOrchestrator.UpdatePaymentMetadata(paymentId, request);

            if (!updatePaymentMetadata)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Could not update Payment Metadata");

            return Ok(updatePaymentMetadata);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "Could not update Payment Metadata");
        }
    }
}
