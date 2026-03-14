using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/english-fractions")]
public class EnglishFractionsController(EnglishFractionsOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> Persist([FromBody] EnglishFractionsRequest request)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var result = await orchestrator.PersistEnglishFractions(request);

        return Ok(result);
    }
}
