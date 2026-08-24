using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/paye-schemes")]
public class PayeSchemesController(FinanceOrchestrator orchestrator) : ControllerBase
{
    [HttpGet("last-submission-date")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetLastSubmissionDate([FromQuery] string empRef)
    {
        try
        {
            var decodedEmpRef = string.IsNullOrEmpty(empRef) ? empRef : Uri.UnescapeDataString(empRef);
            var result = await orchestrator.GetLastSubmissionDateForPayeScheme(decodedEmpRef);
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
}
