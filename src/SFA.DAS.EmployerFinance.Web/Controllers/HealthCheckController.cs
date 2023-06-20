using AutoMapper;
using SFA.DAS.EmployerFinance.Commands.RunHealthCheckCommand;
using SFA.DAS.EmployerFinance.Queries.GetHealthCheck;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.ViewModels;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    //TODO This should be removed
    
    
    public class HealthCheckController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public HealthCheckController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }
        [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
        [HttpGet]
        [Route("healthcheck")]
        public async Task<IActionResult> Index(GetHealthCheckQuery query)
        {
            var response = await _mediator.Send(query);
            var model = _mapper.Map<HealthCheckViewModel>(response);

            return View(model);
        }
        [Authorize(Policy = nameof(PolicyNames.HasEmployerViewerTransactorOwnerAccount))]
        [HttpPost]
        [Route("healthcheck")]
        public async Task<IActionResult> Index(RunHealthCheckCommand command)
        {
            await _mediator.Send(command);

            return RedirectToAction("Index");
        }

        //TODO and replace with proper health check
        [HttpGet]
        [Route("Content/lib/govuk-frontend/dist/assets/images/favicon.ico")]
        public IActionResult FileCheck()
        {
            return Ok();
        }
    }
}