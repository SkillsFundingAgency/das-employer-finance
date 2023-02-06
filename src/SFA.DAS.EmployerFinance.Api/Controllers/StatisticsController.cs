using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.WebApi.Attributes;
using SFA.DAS.EmployerFinance.Queries.GetStatistics;

namespace SFA.DAS.EmployerFinance.Api.Controllers
{
    [DasAuthorize(Roles = "ReadUserAccounts")]
    [Route("api/financestatistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StatisticsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetStatistics()
        {
            var response = await _mediator.Send(new GetStatisticsQuery());
            return Ok(response.Statistics);
        }
    }
}
