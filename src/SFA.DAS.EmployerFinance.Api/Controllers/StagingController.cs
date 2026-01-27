using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Models.PaymentStaging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/payments")]
public class StagingController(StagingOrchestrator orchestrator)
    : ControllerBase
{
    [Route("staging", Name = "BulkPaymentsIngest")]
    [HttpPost]
    public async Task<IActionResult> BulkPaymentsIngest([FromBody] List<PaymentStagingModel> payments)
    {
        return await orchestrator.IngestBulkPayments(payments);
    }

}