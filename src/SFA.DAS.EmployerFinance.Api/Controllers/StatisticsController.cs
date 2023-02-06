using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.WebApi.Attributes;
using SFA.DAS.EmployerFinance.Api.Orchestrators;
using SFA.DAS.EmployerFinance.Queries.GetStatistics;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [DasAuthorize(Roles = "ReadUserAccounts")]
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
