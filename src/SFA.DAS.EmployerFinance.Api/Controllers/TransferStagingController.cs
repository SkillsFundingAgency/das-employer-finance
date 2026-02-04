using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Api.Authorization;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;


[Route("api/transfers")]
public class TransferStagingController(TransferStagingOrchestrator orchestrator) : ControllerBase
{
    [Authorize(Policy = ApiRoles.WriteEmployerAccountBalances)]
    [HttpPost("staging")]
    public async Task<IActionResult> StageTransfers([FromBody] BulkTransferStagingRequest request)
    {
        if (request == null || request.Transfers == null || request.Transfers.Count == 0)
        {
            return BadRequest("Transfers payload is required.");
        }

        var response = await orchestrator.StageTransfers(request);

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