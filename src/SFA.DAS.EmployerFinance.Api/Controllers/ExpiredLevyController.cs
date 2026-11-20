using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/expired-levy")]
public class ExpiredLevyController(ExpiredLevyOrchestrator orchestrator) : ControllerBase
{
    [HttpGet("{accountId:long}")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [ProducesResponseType(typeof(ExpiredLevyResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiredFunds(long accountId, CancellationToken cancellationToken)
    {
        var result = await orchestrator.GetExpiredFunds(accountId, cancellationToken);
        return Ok(result);
    }
}
