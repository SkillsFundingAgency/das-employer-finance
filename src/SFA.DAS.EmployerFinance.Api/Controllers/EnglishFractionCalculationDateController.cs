using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/english-fraction-calculation-date")]
public class EnglishFractionCalculationDateController(EnglishFractionCalculationDateOrchestrator orchestrator) : ControllerBase
{
    [HttpGet("{empRef}")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLastCalculationDate([FromRoute] string empRef)
    {
        var decodedEmpRef = string.IsNullOrEmpty(empRef) ? empRef : Uri.UnescapeDataString(empRef);
        var result = await orchestrator.GetLastCalculationDate(decodedEmpRef);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> Persist([FromBody] EnglishFractionCalculationDateRequest? request)
    {
        if (request is null)
        {
            return BadRequest("Request payload is required.");
        }

        await orchestrator.PersistCalculationDate(request);
        return Ok();
    }
}
