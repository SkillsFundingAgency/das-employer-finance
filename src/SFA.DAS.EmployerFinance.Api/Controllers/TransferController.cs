using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    [Authorize(Policy = ApiRoles.WriteEmployerAccountBalances)]
    [HttpPost("staging")]
    public async Task<IActionResult> StageTransfers([FromBody] BulkTransferStagingRequest request)
    {
        if (request == null || request.Transfers == null || request.Transfers.Count == 0)
        {
            return BadRequest("Transfers payload is required.");
        }

        var response = await transferOrchestrator.StageTransfers(request);

        if (response.HasValidationErrors)
        {
            return BadRequest(response.ValidationErrors);
        }

        if (response.HasConflicts)
        {
            return Conflict(new { transferIds = response.ConflictingTransferIds });
        }

        if (!response.IsSuccess)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while staging transfers.");
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            insertedCount = response.InsertedCount,
            transferIds = response.TransferIds
        });
    }
}
