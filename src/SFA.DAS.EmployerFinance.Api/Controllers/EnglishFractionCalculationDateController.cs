using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/english-fraction-calculation-date")]
public class EnglishFractionCalculationDateController(EnglishFractionCalculationDateOrchestrator orchestrator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> Persist([FromBody] EnglishFractionCalculationDateRequest? request)
    {
        if (request is null)
        {
            return BadRequest("Request payload is required.");
        }

        try
        {
            await orchestrator.PersistCalculationDate(request);
            return Ok();
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
}
