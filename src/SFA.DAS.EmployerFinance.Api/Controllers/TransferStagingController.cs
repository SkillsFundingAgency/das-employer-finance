using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Commands.StagingTransfers;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;


[Route("api/transfers")]
public class TransferStagingController(TransferStagingOrchestrator orchestrator) : ControllerBase
{
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpPost("staging")]
    public async Task<IActionResult> StageTransfers([FromBody] StageTransfersRequest request)
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

        if (response.ConflictingTransferIds.Any())
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
            insertedCount = response.InsertedTransferIds.Count,
            transferIds = response.InsertedTransferIds
        });
    }
}