using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.EmployerUserRoles.Options;
using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.EmployerFinance.Web.Orchestrators;

namespace SFA.DAS.EmployerFinance.Web.Controllers
{
    [DasAuthorize(EmployerUserRole.Any)]
    [Route("accounts/{HashedAccountId}")] 
    public class TransfersController :Controller
    {
        private readonly TransfersOrchestrator _transfersOrchestrator;

        public TransfersController(TransfersOrchestrator transfersOrchestrator)
        {
            _transfersOrchestrator = transfersOrchestrator;
        }

        [HttpGet]
        [Route("transfers")]
        public async Task<IActionResult> Index(string hashedAccountId)
        {
            var viewModel = await _transfersOrchestrator.GetIndexViewModel(hashedAccountId);

            return View(viewModel);
        }

        [DasAuthorize("EmployerFeature.FinanceDetails")]
        [HttpGet]
        [Route("transfers/financial-breakdown")]
        public async Task<IActionResult> FinancialBreakdown(string hashedAccountId)
        {
            var viewModel = await _transfersOrchestrator.GetFinancialBreakdownViewModel(hashedAccountId);
            return View(viewModel);
        }
    }
}