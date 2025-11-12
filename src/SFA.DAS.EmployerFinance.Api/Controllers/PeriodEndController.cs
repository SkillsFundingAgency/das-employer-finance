using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;


[Route("api/period-ends")]
public class PeriodEndController(PeriodEndOrchestrator periodEndOrchestrator) : ControllerBase
{
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await periodEndOrchestrator.GetPeriodEnds();

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpPost()]
    public async Task<IActionResult> Create([FromBody] PeriodEnd request)
    {
        if (request is null)
            return BadRequest();

        var createdPeriodEnd = await periodEndOrchestrator.CreatePeriodEnd(request);

        if (createdPeriodEnd == null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Could not create period end");

        return Ok(createdPeriodEnd);
    }


    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet("{periodEndId}")]
    public async Task<IActionResult> GetByPeriodEndByPeriodEndId(string periodEndId)
    {
        var result = await periodEndOrchestrator.GetPeriodEndByPeriodEndId(periodEndId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
