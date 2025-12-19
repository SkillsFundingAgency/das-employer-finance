using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;
using SFA.DAS.Encoding;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/accounts")]
public class TransferConnectionsController(IMediator mediator, IEncodingService encodingService) : ControllerBase
{
    [HttpGet]
    [Route("{hashedAccountId}/transfers/connections")]
    public async Task<IActionResult> GetTransferConnections(string hashedAccountId)
    {
        var accountId = encodingService.Decode(hashedAccountId, EncodingType.AccountId);

        var response = await mediator.Send(new GetTransferConnectionsQuery { AccountId = accountId });
        return Ok(response.TransferConnections);
    }

    [HttpGet]
    [Route("internal/{accountId}/transfers/connections")]
    public async Task<IActionResult> GetTransferConnections(long accountId, TransferConnectionInvitationStatus status = TransferConnectionInvitationStatus.Approved)
    {
        var response = await mediator.Send(new GetTransferConnectionsQuery
        {
            AccountId = accountId,
            Status = status
        });

        return Ok(response.TransferConnections);
    }

    [HttpGet]
    [Route("{accountId}/transfers/connectionss")]
    public async Task<IActionResult> GetTransfersByPeriodEnd(long accountId,[FromQuery] string periodEnd)
    {
        if (string.IsNullOrWhiteSpace(periodEnd))
        {
            return BadRequest("periodEnd query parameter is required.");
        }

        if (!DateTime.TryParse(periodEnd, out var parsedPeriodEnd))
        {
            return BadRequest("Invalid periodEnd format.");
        }

        var response = await mediator.Send(new GetTransfersByPeriodEndRequest
        {
            AccountId = accountId,
            PeriodEnd = periodEnd
        });

        if (response.AccountTransfers == null || !response.AccountTransfers.Any())
        {
            return NotFound($"No transfers found for account {accountId} and period {periodEnd}.");
        }

        return Ok(response);
    }

}