﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts")]
public class EmployerAccountsController : ControllerBase
{
    private readonly FinanceOrchestrator _financeOrchestrator;

    public EmployerAccountsController(FinanceOrchestrator financeOrchestrator)
    {
        _financeOrchestrator = financeOrchestrator;
    }      

    [Route("balances")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpPost]
    public async Task<IActionResult> GetAccountBalances([FromBody]List<string> accountIds)
    {
        var result = await _financeOrchestrator.GetAccountBalances(accountIds);

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
        var result = await _financeOrchestrator.GetTransferAllowance(hashedAccountId);

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
        var result = await _financeOrchestrator.GetTransferAllowanceByAccountId(accountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet]
    [Route("{accountId}/projection-summary")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    public async Task<IActionResult> GetAccountProjectionSummary(long accountId)
    {
        var result = await _financeOrchestrator.GetAccountProjectionSummary(accountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}