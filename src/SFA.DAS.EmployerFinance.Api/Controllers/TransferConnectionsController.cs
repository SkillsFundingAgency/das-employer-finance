using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.WebApi.Attributes;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Queries.GetTransferConnections;
using SFA.DAS.HashingService;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    [Route("api/accounts")]
    public class TransferConnectionsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHashingService _hashingService;

        public TransferConnectionsController(IMediator mediator, IHashingService hashingService)
        {
            _mediator = mediator;
            _hashingService = hashingService;
        }

        [Route("{hashedAccountId}/transfers/connections")]
        public async Task<IActionResult> GetTransferConnections(string hashedAccountId)
        {
            var accountId = _hashingService.DecodeValue(hashedAccountId);

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