using System.ComponentModel.DataAnnotations;
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

    [HttpGet("{empRef}/period-12-declarations")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetExistingPeriod12LevyDeclarations(string empRef)
    {
        var result = await orchestrator.GetExistingPeriod12LevyDeclarations(DecodeEmpRef(empRef));
        return Ok(result);
    }

    [HttpGet("{empRef}/submission-ids")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetSubmissionIds(string empRef)
    {
        List<string> result = await orchestrator.GetSubmissionIds(DecodeEmpRef(empRef));
        return Ok(result);
    }

    [HttpGet("{empRef}/last-submission-date")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLastSubmissionDate(string empRef)
    {
        var result = await orchestrator.GetLastSubmissionDate(DecodeEmpRef(empRef));
        return Ok(result);
    }

    [HttpGet("{accountId:long}/last-submission-date")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLastSubmissionDate(long accountId)
    {
        var result = await orchestrator.GetLastSubmissionDate(accountId);
        return Ok(new { lastSubmissionDate = result.LastSumissionDate });
    }

    [HttpGet]
    [Route("{accountId:long}/summary")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLevySummary(long accountId)
    {
        var result = await orchestrator.GetLevySummaryByAccountId(accountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{accountId:long}/summaryByDate")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLevyDeclarationSummaryByDate(long accountId,
        [FromQuery, Required] DateOnly fromDate,
        [FromQuery, Required] DateOnly toDate)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await orchestrator.GetLevyDeclarationsByAccountIdAndDateRange(accountId, fromDate, toDate);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result.ToList());
    }

    private static string DecodeEmpRef(string empRef) =>
        string.IsNullOrEmpty(empRef) ? empRef : Uri.UnescapeDataString(empRef);

    private static Dictionary<string, string> GetValidationErrors(ValidationException ex)
    {
        return ex.ValidationResult?.MemberNames
                   .Select(x => x.Split('|', 2))
                   .Where(x => x.Length == 2)
                   .ToDictionary(x => x[0], x => x[1])
               ?? new Dictionary<string, string> { { "Validation", ex.Message } };
    }
}