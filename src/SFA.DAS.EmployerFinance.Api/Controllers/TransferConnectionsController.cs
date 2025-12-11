using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransfersbyPeriodEnd;
using SFA.DAS.Encoding;

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
    public async Task<IActionResult> GetTransfersByPeriodEnd(long accountId, [FromQuery] string periodEnd)
    {
        var response = await mediator.Send(new GetTransfersByPeriodEndRequest
        {
            AccountId = accountId,
            PeriodEnd = periodEnd
        });

        return Ok(response);
    }
}