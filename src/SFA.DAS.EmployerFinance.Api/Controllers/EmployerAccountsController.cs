﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.WebApi.Attributes;
using SFA.DAS.EmployerFinance.Api.Attributes;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [Route("api/accounts")]
    public class EmployerAccountsController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly FinanceOrchestrator _financeOrchestrator;

        public EmployerAccountsController(FinanceOrchestrator financeOrchestrator)
        {
            _financeOrchestrator = financeOrchestrator;
        }      

        [Route("balances")]
        [Authorize(Policy = ApiRoles.ReadAllEmployerAccountBalances)]
        [HttpPost]
        public async Task<IActionResult> GetAccountBalances(List<string> accountIds)
        {
            var result = await _financeOrchestrator.GetAccountBalances(accountIds);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

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
    }
}