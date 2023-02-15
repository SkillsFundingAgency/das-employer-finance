using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.WebApi.Attributes;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [Authorize(Policy = ApiRoles.ReadUserAccounts)]
    [Route("api/financestatistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly StatisticsOrchestrator _statisticsOrchestrator;

        public StatisticsController(StatisticsOrchestrator statisticsController)
        {
            _statisticsOrchestrator = statisticsController;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetStatistics()
        {
            return Ok(await _statisticsOrchestrator.Get());
        }
    }
}
