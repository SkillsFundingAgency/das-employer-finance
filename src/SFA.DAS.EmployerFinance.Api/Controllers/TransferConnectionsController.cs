using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.Encoding;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    [Route("api/accounts")]
    public class TransferConnectionsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IEncodingService _encodingService;

        public TransferConnectionsController(IMediator mediator, IEncodingService encodingService)
        {
            _mediator = mediator;
            _encodingService = encodingService;
        }

        [Route("{hashedAccountId}/transfers/connections")]
        public async Task<IActionResult> GetTransferConnections(string hashedAccountId)
        {
            var accountId = _encodingService.Decode(hashedAccountId,EncodingType.AccountId);

            var response = await _mediator.Send(new GetTransferConnectionsQuery { AccountId = accountId });
            return Ok(response.TransferConnections);
        }

        [Route("internal/{accountId}/transfers/connections")]
        public async Task<IActionResult> GetTransferConnections(long accountId)
        {
            var response = await _mediator.Send(new GetTransferConnectionsQuery { AccountId = accountId });
            return Ok(response.TransferConnections);
        }
    }
}