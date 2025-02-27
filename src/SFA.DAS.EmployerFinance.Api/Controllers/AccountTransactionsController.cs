using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;
using SFA.DAS.EmployerFinance.Helpers;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts/{hashedAccountId}/transactions")]
public class AccountTransactionsController(AccountTransactionsOrchestrator orchestrator, ILinkGeneratorWrapper linkGenerator)
    : ControllerBase
{
    [Route("", Name = "GetTransactionSummary")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> Index(string hashedAccountId)
    {
        var result = await orchestrator.GetAccountTransactionSummary(hashedAccountId);

        if (result == null)
        {
            return NotFound();
        }

        result.ForEach(x =>
        {
            x.Href = linkGenerator.GetPathByName("GetTransactions", new { hashedAccountId, year = x.Year, month = x.Month });
        });

        return Ok(result);
    }

    [Route("{year}/{month}", Name = "GetTransactions")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetTransactions(string hashedAccountId, int year = 0, int month = 0)
    {
        var result = await GetAccountTransactions(hashedAccountId, year, month);

        if (result == null)
        {
            return NotFound();
        }

        if (result.HasPreviousTransactions)
        {
            var previousMonth = new DateTime(result.Year, result.Month, 1).AddMonths(-1);
            result.PreviousMonthUri = linkGenerator.GetPathByName("GetTransactions", new { hashedAccountId, year = previousMonth.Year, month = previousMonth.Month });
        }

        return Ok(result);
    }

    [Route("all-transactions/{year}/{month}", Name = "GetAllTransactionsFrom")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetAllTransactionsFrom(string hashedAccountId, int year = 0, int month = 0)
    {
        var result = await GetAccountTransactions(hashedAccountId, year, month, true);

        if (result == null)
        {
            return NotFound();
        }     

        return Ok(result);
    }

    private async Task<Transactions> GetAccountTransactions(string hashedAccountId, int year, int month, bool getAllTransactions = false)
    {
        if (year == 0)
        {
            year = DateTime.Now.Year;
        }

        if (month == 0)
        {
            month = DateTime.Now.Month;
        }

        return await orchestrator.GetAccountTransactions(hashedAccountId, year, month, getAllTransactions);
    }
}