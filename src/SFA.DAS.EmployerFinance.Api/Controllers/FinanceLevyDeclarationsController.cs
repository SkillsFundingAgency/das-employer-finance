using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/levy-declarations")]
public class FinanceLevyDeclarationsController(LevyDeclarationOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> Persist([FromBody] PersistLevyDeclarationRequestData? request)
    {
        if (request is null)
        {
            return BadRequest("Request payload is required.");
        }

        try
        {
            var result = await orchestrator.PersistLevyDeclarations(request);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(GetValidationErrors(ex));
        }
    }

    private static Dictionary<string, string> GetValidationErrors(ValidationException ex)
    {
        return ex.ValidationResult?.MemberNames
                   .Select(x => x.Split('|', 2))
                   .Where(x => x.Length == 2)
                   .ToDictionary(x => x[0], x => x[1])
               ?? new Dictionary<string, string> { { "Validation", ex.Message } };
    }

    [HttpGet("{empRef}/period-12-declarations")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetExistingPeriod12LevyDeclarations(string empRef)
    {
        var result = await orchestrator.GetExistingPeriod12LevyDeclarations(empRef);
        return Ok(result);
    }

    [HttpGet("{empRef}/submission-ids")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetSubmissionIds(string empRef)
    {
        List<string> result = await orchestrator.GetSubmissionIds(empRef);
        return Ok(result);
    }

    [HttpGet("{empRef}/last-submission-date")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLastSubmissionDate(string empRef)
    {
        var result = await orchestrator.GetLastSubmissionDate(empRef);
        return Ok(result);
    }
}
