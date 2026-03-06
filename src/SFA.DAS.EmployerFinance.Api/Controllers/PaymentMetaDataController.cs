using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.Payments;
using SFA.DAS.Validation.Exceptions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;


[Route("api/payments")]
public class PaymentMetaDataController(PaymentMetaDataOrchestrator paymentOrchestrator) : ControllerBase
{
    [Authorize]
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentMetaDataStaging(Guid paymentId, [FromBody] PaymentMetaDataStaging request)
    {
        if (request is null || paymentId == Guid.Empty)
            return BadRequest();

        try
        {
            var result = await paymentOrchestrator.UpdatePaymentMetaDataStaging(paymentId, request);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("Could not update Payment Metadata Staging");
        }
    }
}
