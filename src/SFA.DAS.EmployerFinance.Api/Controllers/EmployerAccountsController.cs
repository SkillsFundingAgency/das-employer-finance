using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts")]
public class EmployerAccountsController(FinanceOrchestrator financeOrchestrator) : ControllerBase
{
    [Route("balances")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpPost]
    public async Task<IActionResult> GetAccountBalances([FromBody]List<string> accountIds)
    {
        var result = await financeOrchestrator.GetAccountBalances(accountIds);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("{hashedAccountId}/transferAllowance")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetTransferAllowance(string hashedAccountId)
    {
        var result = await financeOrchestrator.GetTransferAllowance(hashedAccountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("{accountId}/transferAllowanceByAccountId")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetTransferAllowanceByAccountId(long accountId)
    {
        var result = await financeOrchestrator.GetTransferAllowanceByAccountId(accountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetAccounts(int pageNumber = 1, int pageSize = 10000)
    {
        var result = await financeOrchestrator.GetAccounts(pageNumber, pageSize);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{accountId}")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetAccountById(long accountId)
    {
        var result = await financeOrchestrator.GetAccountById(accountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{accountId}/payments/ids")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetAccountPaymentIds(long accountId, int pageNumber = 1, int pageSize = 10000)
    {
        var result = await financeOrchestrator.GetAccountPaymentIds(accountId, pageNumber, pageSize);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{accountId}/paye-schemes")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetPayeSchemes(long accountId, [FromQuery] string? source = null)
    {
        var result = await financeOrchestrator.GetPayeSchemesByEmployerId(accountId, source);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("{accountId}/expire-funds")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> ExpireFunds(
        long accountId,
        [FromBody] ExpireFundsRequest request)
    {
        if (request is null)
        {
            return BadRequest("Expire funds payload is required.");
        }

        try
        {
            var response = await financeOrchestrator.ExpireFunds(accountId, request.CorrelationId);
            return Ok(response);
        }
        catch (ValidationException exception)
        {
            return BadRequest(GetValidationErrors(exception));
        }
    }

    private static Dictionary<string, string> GetValidationErrors(ValidationException exception)
    {
        return exception.ValidationResult?.MemberNames
                   .Select(member => member.Split('|', 2))
                   .Where(member => member.Length == 2)
                   .ToDictionary(member => member[0], member => member[1])
               ?? new Dictionary<string, string> { { "Validation", exception.Message } };
    }
}
