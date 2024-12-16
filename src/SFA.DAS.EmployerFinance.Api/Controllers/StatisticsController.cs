using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFinance.Api.Authorization;
using SFA.DAS.EmployerFinance.Api.Orchestrators;

namespace SFA.DAS.EmployerFinance.Api.Controllers;

[Authorize(Policy = ApiRoles.ReadUserAccounts)]
[Route("api/financestatistics")]
public class StatisticsController(StatisticsOrchestrator statisticsController) : ControllerBase
{
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetStatistics()
    {
        return Ok(await statisticsController.Get());
    }
}