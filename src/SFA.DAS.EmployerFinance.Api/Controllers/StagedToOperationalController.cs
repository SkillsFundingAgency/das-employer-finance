using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Commands.TransferStagedToOperational;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/staging")]
public class StagedToOperationalController(IMediator mediator) : ControllerBase
{
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpPost("staged-to-operational")]
    public async Task<IActionResult> StagedToOperational(
        [FromBody] TransferStagedToOperationalRequest request)
    {
        if (request == null)
        {
            return BadRequest("Transfer staged-to-operational payload is required.");
        }

        var response = await mediator.Send(new TransferStagedToOperationalCommand
        {
            AccountId = request.AccountId,
            PeriodEnd = request.PeriodEnd,
            CorrelationId = request.CorrelationId
        });

        if (response.HasValidationErrors)
        {
            return BadRequest(response.ValidationErrors);
        }

        if (!response.IsSuccess)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                isSuccess = false,
                message = response.Message,
                processedCount = response.ProcessedCount
            });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            isSuccess = true,
            message = response.Message,
            processedCount = response.ProcessedCount
        });
    }
}
