using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts")]
public class TransferController(TransferOrchestrator transferOrchestrator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [Route("{accountId}/transfer/connections")]
    public async Task<IActionResult> GetTransfersByPeriodEnd(long accountId, [FromQuery] string periodEnd)
    {
        if (string.IsNullOrWhiteSpace(periodEnd))
        {
            return BadRequest("periodEnd query parameter is required.");
        }

        var response = await transferOrchestrator.GetTransfersByPeriodEnd(accountId, periodEnd);

        if (response.AccountTransfers == null || !response.AccountTransfers.Any())
        {
            return NotFound($"No transfers found for account {accountId} and period {periodEnd}.");
        }

        return Ok(response);
    }
}
