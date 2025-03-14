﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Route("api/accounts/{hashedAccountId}/levy")]
public class FinanceLevyController(FinanceOrchestrator orchestrator) : ControllerBase
{
    [Route("", Name = "GetLevy")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> Index(string hashedAccountId)
    {
        var result = await orchestrator.GetLevy(hashedAccountId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Route("{payrollYear}/{payrollMonth}", Name = "GetLevyForPeriod")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
    {
        var result = await orchestrator.GetLevy(hashedAccountId, payrollYear, payrollMonth);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Route("english-fraction-history", Name = "GetEnglishFractionHistory")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetEnglishFractionHistory(string hashedAccountId, string empRef)
    {
        var result = await orchestrator.GetEnglishFractionHistory(hashedAccountId, empRef);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [Route("english-fraction-current", Name = "GetEnglishFractionCurrent")]
    [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
    [HttpGet]
    public async Task<IActionResult> GetEnglishFractionCurrent([System.Web.Http.FromUri] string[] empRefs, string hashedAccountId)
    {
        var result = await orchestrator.GetEnglishFractionCurrent(hashedAccountId, empRefs);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}