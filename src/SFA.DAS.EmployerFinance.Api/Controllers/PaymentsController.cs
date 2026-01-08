using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class PaymentsController(PaymentsOrchestrator paymentsOrchestrator ) : ControllerBase
{
    [HttpPut("{paymentId}/metadata/staging")]
    public async Task<IActionResult> PaymentsMetadata(string paymentId, [FromBody] Payment payment)
    {
        var result = await paymentsOrchestrator.PutPaymentsMetadata(payment);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
