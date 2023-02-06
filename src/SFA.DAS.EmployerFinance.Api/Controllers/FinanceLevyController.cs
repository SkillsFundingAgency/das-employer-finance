﻿using SFA.DAS.EmployerFinance.Api.Attributes;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [Route("api/accounts/{hashedAccountId}/levy")]
    public class FinanceLevyController : Microsoft.AspNetCore.Mvc.ControllerBase
    {
        private readonly FinanceOrchestrator _orchestrator;

        public FinanceLevyController(FinanceOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Route("", Name = "GetLevy")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IActionResult> Index(string hashedAccountId)
        {
            var result = await _orchestrator.GetLevy(hashedAccountId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Route("{payrollYear}/{payrollMonth}", Name = "GetLevyForPeriod")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IActionResult> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
        {
            var result = await _orchestrator.GetLevy(hashedAccountId, payrollYear, payrollMonth);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Route("english-fraction-history", Name = "GetEnglishFractionHistory")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IActionResult> GetEnglishFractionHistory(string hashedAccountId, string empRef)
        {
            var result = await _orchestrator.GetEnglishFractionHistory(hashedAccountId, empRef);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [Route("english-fraction-current", Name = "GetEnglishFractionCurrent")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IActionResult> GetEnglishFractionCurrent([System.Web.Http.FromUri] string[] empRefs, string hashedAccountId)
        {
            var result = await _orchestrator.GetEnglishFractionCurrent(hashedAccountId, empRefs);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}