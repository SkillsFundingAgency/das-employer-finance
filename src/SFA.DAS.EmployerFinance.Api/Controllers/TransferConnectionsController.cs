﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Models.TransferConnections;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/accounts")]
public class TransferConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEncodingService _encodingService;

    public TransferConnectionsController(IMediator mediator, IEncodingService encodingService)
    {
        _mediator = mediator;
        _encodingService = encodingService;
    }

    [HttpGet]
    [Route("{hashedAccountId}/transfers/connections")]
    public async Task<IActionResult> GetTransferConnections(string hashedAccountId)
    {
        var accountId = _encodingService.Decode(hashedAccountId, EncodingType.AccountId);

        var response = await _mediator.Send(new GetTransferConnectionsQuery { AccountId = accountId });
        return Ok(response.TransferConnections);
    }

    [HttpGet]
    [Route("internal/{accountId}/transfers/connections")]
    public async Task<IActionResult> GetTransferConnections(long accountId, TransferConnectionInvitationStatus status = TransferConnectionInvitationStatus.Approved)
    {
        var response = await _mediator.Send(new GetTransferConnectionsQuery
        {
            AccountId = accountId,
            Status = status
        });

        return Ok(response.TransferConnections);
    }
}