using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Api.Types;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts/{hashedAccountId}/transactions")]
public class AccountTransactionsController : ControllerBase
{
    private readonly AccountTransactionsOrchestrator _orchestrator;
    private readonly IUrlHelper _urlHelper;

    public AccountTransactionsController(AccountTransactionsOrchestrator orchestrator, IUrlHelper urlHelper)
    {
        _orchestrator = orchestrator;
        _urlHelper = urlHelper;
    }

    [Route("", Name = "GetTransactionSummary")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> Index(string hashedAccountId)
    {
        var result = await _orchestrator.GetAccountTransactionSummary(hashedAccountId);

        if (result == null)
        {
            return NotFound();
        }

        result.ForEach(x => x.Href = Url.RouteUrl("GetTransactions", new { hashedAccountId, year = x.Year, month = x.Month }));

        return Ok(result);
    }

    [Route("{year}/{month}", Name = "GetTransactions")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetTransactions(string hashedAccountId, int year, int month)
    {
        var result = await GetAccountTransactions(hashedAccountId, year, month);

        if (result == null)
        {
            return NotFound();
        }

        if (result.HasPreviousTransactions)
        {
            var previousMonth = new DateTime(result.Year, result.Month, 1).AddMonths(-1);
            result.PreviousMonthUri = Url.RouteUrl("GetTransactions", new { hashedAccountId, year = previousMonth.Year, month = previousMonth.Month });
        }

        return Ok(result);
    }

    private async Task<Transactions> GetAccountTransactions(string hashedAccountId, int year, int month)
    {
        if (year == 0)
        {
            year = DateTime.Now.Year;
        }

        if (month == 0)
        {
            month = DateTime.Now.Month;
        }

        var result = await _orchestrator.GetAccountTransactions(hashedAccountId, year, month, _urlHelper);
        return result;
    }
}